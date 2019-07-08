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
using System.Threading;
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

        //bool Can(RequestMethod method);

        Task<IResource> InvokeAsync(Request request);
    }

    public delegate Task<IResource> RequestCallback(Request request);

    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public abstract class ResourceProvider : IResourceProvider
    {
        public static readonly string TagPrefix = "#";

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
            if (Methods is null)
            {
                throw new InvalidOperationException(
                    $"{nameof(Methods)} are not initialized. " +
                    $"You must specify at least one method by initializing this property in the derived type.");
            }

            if (request.Method == RequestMethod.None)
            {
                throw new ArgumentException(paramName: nameof(request), message: $"You must specify a request method. '{RequestMethod.None}' is not one of them.");
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
                        $"An error occured in {ResourceProviderHelper.FormatNames(this)} " +
                        $"while trying to {RequestHelper.FormatMethodName(request)} '{request.Uri}'. " +
                        $"See the inner exception for details.",
                        inner
                    );
                }
            }

            throw DynamicException.Create
            (
                $"MethodNotSupported",
                $"{ResourceProviderHelper.FormatNames(this)} " +
                $"cannot {RequestHelper.FormatMethodName(request)} '{request.Uri}' " +
                $"because it doesn't support this method."
            );
        }

        public virtual void Dispose()
        {
            // Can be overriden when derived.
        }

        public static string CreateTag(string name) => $"{TagPrefix}{name.Trim('#')}";

        

        public class Decorator : IResourceProvider
        {
            public Decorator(IResourceProvider provider)
            {
                Provider = provider;
            }

            protected IResourceProvider Provider { get; }

            public virtual IImmutableContainer Properties => Provider.Properties;

            public virtual IImmutableDictionary<RequestMethod, RequestCallback> Methods => Provider.Methods;

            public virtual Task<IResource> InvokeAsync(Request request) => Provider.InvokeAsync(request);

            public virtual void Dispose() { }
        }
    }
    
    //[UseType, UseMember]
    [Rename(nameof(ResourceProvider))]
    //[PlainSelectorFormatter]
    public class ResourceProviderProperty : SelectorBuilder<ResourceProviderProperty>
    {
        public static readonly Selector<IImmutableSet<SoftString>> Schemes = Select(() => Schemes);

        public static readonly Selector<IImmutableSet<SoftString>> Names = Select(() => Names);

        public static readonly Selector<bool> AllowRelativeUri = Select(() => AllowRelativeUri);
    }

    public class UseProvider : ResourceProvider.Decorator
    {
        private readonly string _name;

        public UseProvider(IResourceProvider provider, string name) : base(provider)
        {
            _name = name;
        }

        public override Task<IResource> InvokeAsync(Request request)
        {
            request.Extensions = request.Extensions.SetName(_name);
            return base.InvokeAsync(request);
        }
    }

    public static class MethodDictionary
    {
        public static IImmutableDictionary<RequestMethod, RequestCallback> Empty { get; } = ImmutableDictionary<RequestMethod, RequestCallback>.Empty;
    }

    public class Request
    {
        [NotNull]
        public UriString Uri { get; set; } = new UriString($"{UriSchemes.Custom.IOnymous}:///");

        [NotNull]
        public RequestMethod Method { get; set; } = RequestMethod.None;

        [NotNull]
        public IImmutableContainer Extensions { get; set; } = ImmutableContainer.Empty;

        [CanBeNull]
        public Func<Task<Stream>> CreateBodyStreamFunc { get; set; }

        #region Methods

        public class Get : Request
        {
            public Get(UriString uri)
            {
                Uri = uri;
                Method = RequestMethod.Get;
            }
        }

        public class Post : Request
        {
            public Post(UriString uri)
            {
                Uri = uri;
                Method = RequestMethod.Post;
            }
        }

        public class Put : Request
        {
            public Put(UriString uri)
            {
                Uri = uri;
                Method = RequestMethod.Put;
            }
        }

        public class Delete : Request
        {
            public Delete(UriString uri)
            {
                Uri = uri;
                Method = RequestMethod.Delete;
            }
        }

        #endregion

        //[UseType, UseMember]
        [Rename(nameof(Request))]
        //[PlainSelectorFormatter]
        public class Property : SelectorBuilder<Property>
        {
            public static readonly Selector<CancellationToken> CancellationToken = Select(() => CancellationToken);
        }
    }

    public static class RequestExtensions
    {
        public static Request SetProperties(this Request request, Func<IImmutableContainer, IImmutableContainer> properties)
        {
            request.Extensions = properties(request.Extensions);
            return request;
        }

        public static Request SetCreateBodyStream(this Request request, Func<Task<Stream>> createBodyStream)
        {
            request.CreateBodyStreamFunc = createBodyStream;
            return request;
        }

        public static Task<Stream> CreateBodyStreamAsync(this Request request)
        {
            if (request.CreateBodyStreamFunc is null)
            {
                throw new ArgumentNullException(
                    paramName: $"{nameof(request)}.{nameof(request.CreateBodyStreamFunc)}",
                    message: $"{FormatMethodName()} request to '{request.Uri}' requires a body that is missing.");
            }

            return request.CreateBodyStreamFunc();

            string FormatMethodName() => request.Method.Name.ToString().ToUpper();
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

    public static class ResourceProviderHelper
    {
        public static string FormatNames(IResourceProvider provider)
        {
            var name = provider.Properties.GetNames().Take(1).Select(x => x.ToString());
            var tags = provider.Properties.GetNames().Skip(1).Select(x => x.ToString());

            return $"{name} [{tags.Join(", ")}]";
        }
    }

    public static class RequestHelper
    {
        public static string FormatMethodName(Request request) => request.Method.Name.ToString().ToUpper();
    }

    public static class ElementOrder
    {
        public const int Preserve = -1; // Less than zero - x is less than y.
        public const int Ignore = 0; // Zero - x equals y.
        public const int Reorder = 1; // Greater than zero - x is greater than y.
    }

    public static class ComparerHelper
    {
        public static bool TryCompareReference<T>(T x, T y, out int referenceOrder)
        {
            if (ReferenceEquals(x, y))
            {
                referenceOrder = ElementOrder.Ignore;
                return true;
            }

            if (ReferenceEquals(x, null))
            {
                referenceOrder = ElementOrder.Preserve;
                return true;
            }

            if (ReferenceEquals(y, null))
            {
                referenceOrder = ElementOrder.Reorder;
                return true;
            }

            referenceOrder = ElementOrder.Ignore;
            return false;
        }
    }

    public class ResourceProviderNameComparer : IComparer<SoftString>
    {
        public int Compare(SoftString x, SoftString y)
        {
            if (ComparerHelper.TryCompareReference(x, y, out var referenceOrder))
            {
                return referenceOrder;
            }

            if (IsTag(x) ^ IsTag(y))
            {
                if (IsTag(x)) return ElementOrder.Reorder;
                if (IsTag(y)) return ElementOrder.Preserve;
            }

            return SoftString.Comparer.Compare(x, y);
        }

        private static bool IsTag(SoftString name) => name.StartsWith(ResourceProvider.TagPrefix);
    }
}