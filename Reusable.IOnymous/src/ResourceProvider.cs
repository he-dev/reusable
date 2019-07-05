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
using Requests = System.Collections.Immutable.IImmutableDictionary<Reusable.IOnymous.ResourceRequestMethod, Reusable.IOnymous.RequestCallback>;

namespace Reusable.IOnymous
{
    [PublicAPI]
    public interface IResourceProvider : IDisposable
    {
        [NotNull]
        IImmutableSession Properties { get; }

        bool CanGet { get; }

        bool CanPost { get; }

        bool CanPut { get; }

        bool CanDelete { get; }

        bool Can(ResourceRequestMethod method);

        Task<IResource> InvokeAsync(ResourceRequest request);

        [ItemNotNull]
        Task<IResource> GetAsync([NotNull] UriString uri, [CanBeNull] IImmutableSession metadata = default);

        [ItemNotNull]
        Task<IResource> PostAsync([NotNull] UriString uri, [NotNull] Stream value, [CanBeNull] IImmutableSession metadata = default);

        [ItemNotNull]
        Task<IResource> PutAsync([NotNull] UriString uri, [NotNull] Stream value, [CanBeNull] IImmutableSession metadata = default);

        [ItemNotNull]
        Task<IResource> DeleteAsync([NotNull] UriString uri, [CanBeNull] IImmutableSession metadata = default);
    }

    public delegate Task<IResource> RequestCallback(ResourceRequest request);

    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public abstract class ResourceProvider : IResourceProvider
    {
        private static readonly From<IProviderMeta> PropertySelector = From<IProviderMeta>.This;

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
                x.Provider.Properties.GetItemOrDefault(PropertySelector.Select(m => m.AllowRelativeUri), false) ||
                x.Uri.Scheme
            ).WithMessage(x => $"{ProviderInfo(x.Provider)} cannot {x.Method.ToUpper()} '{x.Uri}' because it supports only absolute URIs.");
        });

        protected ResourceProvider([NotNull] IImmutableSession properties)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            if (!properties.GetSchemes().Any())
            {
                throw new ArgumentException
                (
                    paramName: nameof(properties),
                    message: $"{GetType().ToPrettyString().ToSoftString()} must specify at least one scheme."
                );
            }

            Properties = properties.SetName(GetType().ToPrettyString().ToSoftString());
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplayValues(p => p.Properties.GetNames(), x => x.ToString());
            builder.DisplayValues(p => p.Properties.GetSchemes(), x => x.ToString());
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

        public virtual IImmutableSession Properties { get; }

        protected IDictionary<ResourceRequestMethod, RequestCallback> Methods { get; } = new Dictionary<ResourceRequestMethod, RequestCallback>();

        #region Wrappers

        public bool Can(ResourceRequestMethod method) => Methods.ContainsKey(method);

        public async Task<IResource> InvokeAsync(ResourceRequest request)
        {
            if (Methods.TryGetValue(request.Method, out var method))
            {
                try
                {
                    return await method(request);
                }
                catch (Exception inner)
                {
                    throw DynamicException.Create
                    (
                        $"Request",
                        $"An error occured in {FormatProviderNames()} " +
                        $"while trying to {FormatMethodName()} '{request.Uri}'. " +
                        $"See the inner exception for details.",
                        inner
                    );
                }
            }

            throw DynamicException.Create
            (
                $"MethodNotSupported",
                $"{FormatProviderNames()} " +
                $"cannot {FormatMethodName()} '{request.Uri}' " +
                $"because it doesn't support this method."
            );

            string FormatProviderNames() => Properties.GetNames().Select(x => x.ToString()).Join("/");

            string FormatMethodName() => request.Method.Name.ToString().ToUpper();
        }

        // These wrappers are to provide helpful exceptions.        

        public async Task<IResource> GetAsync(UriString uri, IImmutableSession metadata = default)
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

        public async Task<IResource> PostAsync(UriString uri, Stream value, IImmutableSession metadata = default)
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

        public async Task<IResource> PutAsync(UriString uri, Stream value, IImmutableSession metadata = default)
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

        public async Task<IResource> DeleteAsync(UriString uri, IImmutableSession metadata = default)
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

        protected virtual Task<IResource> GetAsyncInternal(UriString uri, IImmutableSession metadata) => throw MethodNotSupportedException(uri);

        protected virtual Task<IResource> PostAsyncInternal(UriString uri, Stream value, IImmutableSession metadata) => throw MethodNotSupportedException(uri);

        protected virtual Task<IResource> PutAsyncInternal(UriString uri, Stream value, IImmutableSession metadata) => throw MethodNotSupportedException(uri);

        protected virtual Task<IResource> DeleteAsyncInternal(UriString uri, IImmutableSession metadata) => throw MethodNotSupportedException(uri);

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

        protected static string ProviderInfo(IResourceProvider provider) => provider.Properties.GetNames().Select(n => n.ToString()).Join("/");

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

    public class ResourceRequest
    {
        [NotNull]
        public IImmutableSession Properties { get; set; } = ImmutableSession.Empty;

        [NotNull]
        public ResourceRequestMethod Method { get; set; } = ResourceRequestMethod.None;

        [NotNull]
        public UriString Uri { get; set; } = new UriString(string.Empty);

        [CanBeNull]
        public Stream Body { get; set; }
    }

    public class ResourceRequestMethod : Option<ResourceRequestMethod>
    {
        public ResourceRequestMethod(SoftString name, IImmutableSet<SoftString> values) : base(name, values) { }

        public static readonly ResourceRequestMethod Get = CreateWithCallerName();

        public static readonly ResourceRequestMethod Post = CreateWithCallerName();

        public static readonly ResourceRequestMethod Put = CreateWithCallerName();

        public static readonly ResourceRequestMethod Delete = CreateWithCallerName();
    }
}