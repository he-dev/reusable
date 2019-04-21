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
                x.Metadata.AllowRelativeUri() ||
                x.Provider.Schemes.Contains(DefaultScheme) ||
                x.Provider.Schemes.Contains(x.Uri.Scheme)
            ).WithMessage(x => $"{ProviderInfo(x.Provider)} cannot {x.Method.ToUpper()} '{x.Uri}' because it supports only such schemes as [{x.Provider.Schemes.Join(", ")}].");

            builder.True
            (x =>
                x.Metadata.AllowRelativeUri() ||
                x.Uri.Scheme
            ).WithMessage(x => $"{ProviderInfo(x.Provider)} cannot {x.Method.ToUpper()} '{x.Uri}' because it supports only absolute URIs.");

            builder.True
            (x =>
                x.Metadata.Resource().Format().IsNotNull()
            ).WithMessage(x => $"{ProviderInfo(x.Provider)} cannot {x.Method.ToUpper()} '{x.Uri}' because it requires resource format specified by the metadata.");

            builder.True
            (x =>
                x.Metadata.Resource().Format().IsNotNull()
            ).WithMessage(x => $"{ProviderInfo(x.Provider)} cannot {x.Method.ToUpper()} '{x.Uri}' because it requires resource format specified by the metadata.");

            string ProviderInfo(IResourceProvider provider)
            {
                return new[]
                {
                    provider.Metadata.Provider().DefaultName().ToString(),
                    provider.Metadata.Provider().CustomName().ToString(),
                }.Where(Conditional.IsNotNullOrEmpty).Join("/");
            }
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
            builder.DisplayMember(p => p.Metadata.Provider().DefaultName());
            builder.DisplayMember(p => p.Metadata.Provider().CustomName());
            builder.DisplayMember(x => x.Schemes);
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
            RequestValidator.Validate(new Request
            {
                Method = ExtractMethodName(nameof(GetAsync)),
                Provider = this,
                Uri = uri,
                Metadata = metadata,
                Stream = default
            }).Assert();

            ValidateSchemeNotEmpty(uri);
            ValidateSchemeMatches(uri);

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
            ValidateSchemeNotEmpty(uri);
            ValidateSchemeMatches(uri);

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
            ValidateSchemeNotEmpty(uri);
            ValidateSchemeMatches(uri);

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
            ValidateSchemeNotEmpty(uri);
            ValidateSchemeMatches(uri);

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

        #region Validations

        protected void ValidateSchemeMatches([NotNull] UriString uri, [CallerMemberName] string memberName = null)
        {
            if (Metadata.AllowRelativeUri())
            {
                return;
            }

            if (Schemes.Contains(DefaultScheme))
            {
                return;
            }

            if (!Schemes.Contains(uri.Scheme))
            {
                throw DynamicException.Create
                (
                    "InvalidScheme",
                    Because(memberName, uri, $"it requires uri to specify scheme [{Schemes.Join(",")}].")
                );
            }
        }

        protected void ValidateSchemeNotEmpty([NotNull] UriString uri, [CallerMemberName] string memberName = null)
        {
            if (Metadata.AllowRelativeUri())
            {
                return;
            }

            if (!uri.Scheme)
            {
                throw DynamicException.Create
                (
                    "MissingScheme",
                    Because(memberName, uri, "it must contain scheme.")
                );
            }
        }

        protected void ValidateFormatNotNull<T>(T fileProvider, UriString uri, Metadata metadata, [CallerMemberName] string memberName = null) where T : IResourceProvider
        {
            if (metadata.Resource().Format().IsNull)
            {
                throw new ArgumentException
                (
                    paramName: nameof(metadata),
                    message: ResourceHelper.FormatMessage<T>(memberName, uri, $"you need to specify file format via {nameof(metadata)}.")
                );
            }
        }

        protected Exception MethodNotSupportedException(UriString uri, [CallerMemberName] string memberName = null)
        {
            return DynamicException.Create
            (
                $"{ExtractMethodName(memberName)}NotSupported",
                Because(memberName, uri, "it doesn't support it.")
            );
        }

        #endregion

        #region Helpers

        protected Exception WrapException(UriString uri, Metadata metadata, Exception inner, [CallerMemberName] string memberName = null)
        {
            throw DynamicException.Create
            (
                ExtractMethodName(memberName),
                Because(memberName, uri, "of an error. See inner exception for details."),
                inner
            );
        }

        private string ExtractMethodName(string memberName)
        {
            return Regex.Match(memberName, @"^(?<method>\w+)Async").Groups["method"].Value;
        }

        private string Because(string memberName, UriString uri, string reason)
        {
            return $"{GetType().ToPrettyString()} cannot {ExtractMethodName(memberName).ToUpper()} '{uri}' because {reason}.";
        }

        #endregion

        public virtual void Dispose()
        {
            // Can be overriden when derived.
        }

        #region operators

        //public static ResourceProvider operator +(ResourceProvider decorable, Func<ResourceProvider, ResourceProvider> decorator) => decorator(decorable);

        #endregion

        private class Request
        {
            public string Method { get; set; }

            public IResourceProvider Provider { get; set; }

            public UriString Uri { get; set; }

            public Metadata Metadata { get; set; }

            public Stream Stream { get; set; }
        }
    }
}