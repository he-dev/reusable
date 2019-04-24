using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Flawless;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public interface IResourceProvider : IDisposable
    {
        [NotNull]
        Metadata Metadata { get; }

        IImmutableSet<SoftString> Schemes { get; }

        bool CanGet { get; }

        bool CanPost { get; }

        bool CanPut { get; }

        bool CanDelete { get; }

        [ItemNotNull]
        Task<IResourceInfo> GetAsync([NotNull] UriString uri, Metadata metadata = default);

        [ItemNotNull]
        Task<IResourceInfo> PostAsync([NotNull] UriString uri, [NotNull] Stream value, Metadata metadata = default);

        [ItemNotNull]
        Task<IResourceInfo> PutAsync([NotNull] UriString uri, [NotNull] Stream value, Metadata metadata = default);

        [ItemNotNull]
        Task<IResourceInfo> DeleteAsync([NotNull] UriString uri, Metadata metadata = default);
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class ResourceProvider : IResourceProvider
    {
        public static readonly SoftString DefaultScheme = "ionymous";

        // Because: $"{GetType().ToPrettyString()} cannot {ExtractMethodName(memberName).ToUpper()} '{uri}' because {reason}.";
        private static readonly IExpressValidator<Request> RequestValidator = ExpressValidator.For<Request>(builder =>
        {
            builder.True
            (x =>
                x.Provider.Metadata.AllowRelativeUri() ||
                x.Provider.Schemes.Contains(DefaultScheme) ||
                x.Provider.Schemes.Contains(x.Uri.Scheme)
            ).WithMessage(x => $"{ProviderInfo(x.Provider)} cannot {x.Method.ToUpper()} '{x.Uri}' because it supports only such schemes as [{x.Provider.Schemes.Join(", ")}].");

            builder.True
            (x =>
                x.Provider.Metadata.AllowRelativeUri() ||
                x.Uri.Scheme
            ).WithMessage(x => $"{ProviderInfo(x.Provider)} cannot {x.Method.ToUpper()} '{x.Uri}' because it supports only absolute URIs.");
        });


        protected ResourceProvider([NotNull] IEnumerable<SoftString> schemes, Metadata metadata)
        {
            if (schemes == null) throw new ArgumentNullException(nameof(schemes));

            //var metadata = Metadata.Empty;

            // If this is a decorator then the decorated resource-provider already has set this.
            if (!metadata.Provider().DefaultName())
            {
                metadata = metadata.Provider(s => s.DefaultName(GetType().ToPrettyString()));
            }

            if ((Schemes = schemes.ToImmutableHashSet()).Empty())
            {
                throw new ArgumentException(paramName: nameof(metadata), message: $"{nameof(schemes)} must not be empty.");
            }

            Metadata = metadata;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            //builder.DisplayCollection(p => p.ProviderNames());
            builder.DisplayValue(p => p.Metadata.Provider().DefaultName());
            builder.DisplayValue(p => p.Metadata.Provider().CustomName());
            builder.DisplayValue(x => x.Schemes);
        });

        public bool CanGet => Implements(nameof(GetAsyncInternal));

        public bool CanPost => Implements(nameof(PostAsyncInternal));

        public bool CanPut => Implements(nameof(PutAsyncInternal));

        public bool CanDelete => Implements(nameof(DeleteAsyncInternal));

        private bool Implements(string methodName)
        {
            // ReSharper disable once PossibleNullReferenceException - nope, not true in this case
            return GetType().GetMethod(methodName, BindingFlags.NonPublic | BindingFlags.Instance).DeclaringType == GetType();
        }

        public virtual Metadata Metadata { get; }

        public virtual IImmutableSet<SoftString> Schemes { get; }

        #region Wrappers

        // These wrappers are to provide helpful exceptions.        

        public async Task<IResourceInfo> GetAsync(UriString uri, Metadata metadata = default)
        {
            ValidateRequest(ExtractMethodName(nameof(GetAsync)), uri, metadata, Stream.Null, RequestValidator);

            try
            {
                return await GetAsyncInternal(uri, metadata);
            }
            catch (Exception inner)
            {
                throw WrapException(uri, metadata, inner);
            }
        }

        public async Task<IResourceInfo> PostAsync(UriString uri, Stream value, Metadata metadata = default)
        {
            ValidateRequest(ExtractMethodName(nameof(PostAsync)), uri, metadata, value, RequestValidator);

            try
            {
                return await PostAsyncInternal(uri, value, metadata);
            }
            catch (Exception inner)
            {
                throw WrapException(uri, metadata, inner);
            }
        }

        public async Task<IResourceInfo> PutAsync(UriString uri, Stream value, Metadata metadata = default)
        {
            ValidateRequest(ExtractMethodName(nameof(PutAsync)), uri, metadata, value, RequestValidator);

            try
            {
                return await PutAsyncInternal(uri, value, metadata);
            }
            catch (Exception inner)
            {
                throw WrapException(uri, metadata, inner);
            }
        }

        public async Task<IResourceInfo> DeleteAsync(UriString uri, Metadata metadata = default)
        {
            ValidateRequest(ExtractMethodName(nameof(GetAsync)), uri, metadata, Stream.Null, RequestValidator);

            try
            {
                return await DeleteAsyncInternal(uri, metadata);
            }
            catch (Exception inner)
            {
                throw WrapException(uri, metadata, inner);
            }
        }

        #endregion

        #region Internal

        protected virtual Task<IResourceInfo> GetAsyncInternal(UriString uri, Metadata metadata) => throw MethodNotSupportedException(uri);

        protected virtual Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, Metadata metadata) => throw MethodNotSupportedException(uri);

        protected virtual Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, Metadata metadata) => throw MethodNotSupportedException(uri);

        protected virtual Task<IResourceInfo> DeleteAsyncInternal(UriString uri, Metadata metadata) => throw MethodNotSupportedException(uri);

        #endregion

        #region Helpers

        protected void ValidateRequest(string method, UriString uri, Metadata metadata, Stream stream, params IExpressValidator<Request>[] validators)
        {
            var request = new Request
            {
                Method = method,
                Provider = this,
                Uri = uri,
                Metadata = metadata,
                Stream = default
            };

            foreach (var validator in validators)
            {
                validator.Validate(request).Assert();
            }
        }

        private Exception MethodNotSupportedException(UriString uri, [CallerMemberName] string memberName = null)
        {
            return DynamicException.Create
            (
                $"{ExtractMethodName(memberName)}NotSupported",
                Because(memberName, uri, "it doesn't support it.")
            );
        }

        private Exception WrapException(UriString uri, Metadata metadata, Exception inner, [CallerMemberName] string memberName = null)
        {
            throw DynamicException.Create
            (
                ExtractMethodName(memberName),
                Because(memberName, uri, "of an error. See inner exception for details."),
                inner
            );
        }

        protected static string ExtractMethodName(string memberName)
        {
            return Regex.Match(memberName, @"^(?<method>\w+)Async").Groups["method"].Value;
        }

        private string Because(string memberName, UriString uri, string reason)
        {
            return $"{GetType().ToPrettyString()} cannot {ExtractMethodName(memberName).ToUpper()} '{uri}' because {reason}.";
        }

        protected static string ProviderInfo(IResourceProvider provider)
        {
            return new[]
            {
                provider.Metadata.Provider().DefaultName().ToString(),
                provider.Metadata.Provider().CustomName().ToString(),
            }.Where(Conditional.IsNotNullOrEmpty).Join("/");
        }

        #endregion

        public virtual void Dispose()
        {
            // Can be overriden when derived.
        }

        #region operators

        //public static ResourceProvider operator +(ResourceProvider decorable, Func<ResourceProvider, ResourceProvider> decorator) => decorator(decorable);

        #endregion

        protected class Request
        {
            public string Method { get; set; }

            public IResourceProvider Provider { get; set; }

            public UriString Uri { get; set; }

            public Metadata Metadata { get; set; }

            public Stream Stream { get; set; }
        }
    }
}