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
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Flawless;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public interface IResourceProvider : IDisposable
    {
        [NotNull]
        IImmutableSession Metadata { get; }

        bool CanGet { get; }

        bool CanPost { get; }

        bool CanPut { get; }

        bool CanDelete { get; }

        [ItemNotNull]
        Task<IResourceInfo> GetAsync([NotNull] UriString uri, [CanBeNull] IImmutableSession metadata = default);

        [ItemNotNull]
        Task<IResourceInfo> PostAsync([NotNull] UriString uri, [NotNull] Stream value, [CanBeNull] IImmutableSession metadata = default);

        [ItemNotNull]
        Task<IResourceInfo> PutAsync([NotNull] UriString uri, [NotNull] Stream value, [CanBeNull] IImmutableSession metadata = default);

        [ItemNotNull]
        Task<IResourceInfo> DeleteAsync([NotNull] UriString uri, [CanBeNull] IImmutableSession metadata = default);
    }

    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public abstract class ResourceProvider : IResourceProvider
    {
        // Because: $"{GetType().ToPrettyString()} cannot {ExtractMethodName(memberName).ToUpper()} '{uri}' because {reason}.";
        private static readonly IExpressValidator<Request> RequestValidator = ExpressValidator.For<Request>(builder =>
        {
//            builder.True
//            (x =>
//                x.Provider.Metadata.GetItemOrDefault(From<IProviderMeta>.Select(m => m.AllowRelativeUri), false) ||
//                x.Provider.Schemes.Contains(ResourceSchemes.IOnymous) ||
//                x.Provider.Schemes.Contains(x.Uri.Scheme)
//            ).WithMessage(x => $"{ProviderInfo(x.Provider)} cannot {x.Method.ToUpper()} '{x.Uri}' because it supports only such schemes as [{x.Provider.Schemes.Join(", ")}].");

            builder.True
            (x =>
                x.Provider.Metadata.GetItemOrDefault(From<IProviderMeta>.Select(m => m.AllowRelativeUri), false) ||
                x.Uri.Scheme
            ).WithMessage(x => $"{ProviderInfo(x.Provider)} cannot {x.Method.ToUpper()} '{x.Uri}' because it supports only absolute URIs.");
        });

        protected ResourceProvider([NotNull] IImmutableSession metadata)
        {
            if (metadata == null) throw new ArgumentNullException(nameof(metadata));

            if (!metadata.GetSchemes().Any())
            {
                throw new ArgumentException
                (
                    paramName: nameof(metadata),
                    message: $"{GetType().ToPrettyString().ToSoftString()} must specify at least one scheme."
                );
            }

            Metadata =
                metadata
                    .SetName(GetType().ToPrettyString().ToSoftString());
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayValues(p => p.Metadata.GetNames(), x => x.ToString());
            builder.DisplayValues(p => p.Metadata.GetSchemes(), x => x.ToString());
            //builder.DisplayValues(p => Names);
            //builder.DisplayValue(x => x.Schemes);
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

        public virtual IImmutableSession Metadata { get; }

        #region Wrappers

        // These wrappers are to provide helpful exceptions.        

        public async Task<IResourceInfo> GetAsync(UriString uri, IImmutableSession metadata = default)
        {
            ValidateRequest(ExtractMethodName(nameof(GetAsync)), uri, metadata, Stream.Null, RequestValidator);

            try
            {
                return await GetAsyncInternal(uri, metadata ?? ImmutableSession.Empty);
            }
            catch (Exception inner)
            {
                throw WrapException(uri, metadata, inner);
            }
        }

        public async Task<IResourceInfo> PostAsync(UriString uri, Stream value, IImmutableSession metadata = default)
        {
            ValidateRequest(ExtractMethodName(nameof(PostAsync)), uri, metadata ?? ImmutableSession.Empty, value, RequestValidator);

            try
            {
                return await PostAsyncInternal(uri, value, metadata);
            }
            catch (Exception inner)
            {
                throw WrapException(uri, metadata, inner);
            }
        }

        public async Task<IResourceInfo> PutAsync(UriString uri, Stream value, IImmutableSession metadata = default)
        {
            ValidateRequest(ExtractMethodName(nameof(PutAsync)), uri, metadata ?? ImmutableSession.Empty, value, RequestValidator);

            try
            {
                return await PutAsyncInternal(uri, value, metadata);
            }
            catch (Exception inner)
            {
                throw WrapException(uri, metadata, inner);
            }
        }

        public async Task<IResourceInfo> DeleteAsync(UriString uri, IImmutableSession metadata = default)
        {
            ValidateRequest(ExtractMethodName(nameof(GetAsync)), uri, metadata ?? ImmutableSession.Empty, Stream.Null, RequestValidator);

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

        protected virtual Task<IResourceInfo> GetAsyncInternal(UriString uri, IImmutableSession metadata) => throw MethodNotSupportedException(uri);

        protected virtual Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, IImmutableSession metadata) => throw MethodNotSupportedException(uri);

        protected virtual Task<IResourceInfo> PutAsyncInternal(UriString uri, Stream value, IImmutableSession metadata) => throw MethodNotSupportedException(uri);

        protected virtual Task<IResourceInfo> DeleteAsyncInternal(UriString uri, IImmutableSession metadata) => throw MethodNotSupportedException(uri);

        #endregion

        #region Helpers

        protected void ValidateRequest(string method, UriString uri, IImmutableSession metadata, Stream stream, params IExpressValidator<Request>[] validators)
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

        private Exception WrapException(UriString uri, IImmutableSession metadata, Exception inner, [CallerMemberName] string memberName = null)
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

        protected static string ProviderInfo(IResourceProvider provider) => provider.Metadata.GetNames().Select(n => n.ToString()).Join("/");

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

            public IImmutableSession Metadata { get; set; }

            public Stream Stream { get; set; }
        }
    }
}