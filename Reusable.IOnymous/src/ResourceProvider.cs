using System;
using System.Collections;
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
        IImmutableContainer Properties { get; }

        IImmutableDictionary<RequestMethod, RequestCallback> Methods { get; }

        bool Can(RequestMethod method);

        Task<IResource> InvokeAsync(Request request);
    }

    public delegate Task<IResource> RequestCallback(Request request);

    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public abstract class ResourceProvider : IResourceProvider
    {
        public static readonly From<IProviderProperties> PropertySelector = From<IProviderProperties>.This;

        // Because: $"{GetType().ToPrettyString()} cannot {ExtractMethodName(memberName).ToUpper()} '{uri}' because {reason}.";
        // private static readonly IExpressValidator<Request> RequestValidator = ExpressValidator.For<Request>(builder =>
        // {
        //     //            builder.True
        //     //            (x =>
        //     //                x.Provider.Metadata.GetItemOrDefault(From<IProviderMeta>.Select(m => m.AllowRelativeUri), false) ||
        //     //                x.Provider.Schemes.Contains(ResourceSchemes.IOnymous) ||
        //     //                x.Provider.Schemes.Contains(x.Uri.Scheme)
        //     //            ).WithMessage(x => $"{ProviderInfo(x.Provider)} cannot {x.Method.ToUpper()} '{x.Uri}' because it supports only such schemes as [{x.Provider.Schemes.Join(", ")}].");
        //
        //     builder.True
        //     (x =>
        //         x.Provider.Properties.GetItemOrDefault(PropertySelector.Select(m => m.AllowRelativeUri), false) ||
        //         x.Uri.Scheme
        //     ).WithMessage(x => $"{ProviderInfo(x.Provider)} cannot {x.Method.ToUpper()} '{x.Uri}' because it supports only absolute URIs.");
        // });

        protected ResourceProvider([NotNull] IImmutableContainer properties)
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

        public virtual IImmutableContainer Properties { get; }

        public IImmutableDictionary<RequestMethod, RequestCallback> Methods { get; protected set; }

        public bool Can(RequestMethod method) => Methods.ContainsKey(method);

        public virtual async Task<IResource> InvokeAsync(Request request)
        {
            if (request.Method == RequestMethod.None)
            {
                throw new ArgumentException(paramName: nameof(request), message: "You must set the request method.");
            }

            if (Methods.TryGetValue(request.Method, out var method))
            {
                try
                {
                    return new ResourceExceptionHandler(await method(request));
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

        public virtual void Dispose()
        {
            // Can be overriden when derived.
        }

        #region operators

        //public static ResourceProvider operator +(ResourceProvider decorable, Func<ResourceProvider, ResourceProvider> decorator) => decorator(decorable);

        #endregion
    }

    public static class MethodDictionary
    {
        public static IImmutableDictionary<RequestMethod, RequestCallback> Empty => ImmutableDictionary<RequestMethod, RequestCallback>.Empty;
    }

    public class Request
    {
        public static readonly From<IRequestProperties> PropertySelector = From<IRequestProperties>.This;

        [NotNull]
        public UriString Uri { get; set; } = new UriString($"{ResourceSchemes.IOnymous}:///");

        [NotNull]
        public RequestMethod Method { get; set; } = RequestMethod.None;

        [NotNull]
        public IImmutableContainer Properties { get; set; } = ImmutableContainer.Empty;

        [CanBeNull]
        public Stream Body { get; set; }

        public class Get : Request
        {
            public Get(UriString uri)
            {
                Uri = uri;
                Method = RequestMethod.Get;
            }
        }
    }

    public class RequestMethod : Option<RequestMethod>
    {
        public RequestMethod(SoftString name, IImmutableSet<SoftString> values) : base(name, values) { }

        public static readonly RequestMethod Get = CreateWithCallerName();

        public static readonly RequestMethod Post = CreateWithCallerName();

        public static readonly RequestMethod Put = CreateWithCallerName();

        public static readonly RequestMethod Delete = CreateWithCallerName();
    }
}