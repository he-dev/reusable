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
    public interface IResourceController : IDisposable
    {
        [NotNull]
        IImmutableContainer Properties { get; }
    }

    public delegate Task<IResource> InvokeCallback(Request request);

    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public abstract class ResourceController : IResourceController
    {
        protected ResourceController([NotNull] IImmutableContainer properties)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            if (properties.GetItemOrDefault(ResourceControllerProperties.Schemes) is var schemes && (schemes is null || !schemes.Any()))
            {
                throw new ArgumentException
                (
                    paramName: nameof(properties),
                    message: $"{GetType().ToPrettyString().ToSoftString()} must specify at least one scheme."
                );
            }

            Properties = properties.UpdateItem(ResourceControllerProperties.Tags, tags => tags.Add(GetType().ToPrettyString().ToSoftString()));
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            //builder.DisplayEnumerable(p => p.Properties.Tags(), x => x.ToString());
            //builder.DisplayEnumerable(p => p.Properties.GetSchemes(), x => x.ToString());
            //builder.DisplayValues(p => Names);
            //builder.DisplayValue(x => x.Schemes);
        });

        public virtual IImmutableContainer Properties { get; }

        protected IResource DoesNotExist(Request request) => Resource.DoesNotExist.FromRequest(request);

        // Can be overriden when derived.
        public virtual void Dispose() { }
    }

    public static class ResourceProviderExtensions
    {
        public static bool SupportsRelativeUri(this IResourceController resourceController)
        {
            return resourceController.Properties.GetItemOrDefault(ResourceControllerProperties.SupportsRelativeUri);
        }
    }

    [UseType, UseMember]
    [PlainSelectorFormatter]
    [Rename(nameof(ResourceController))]
    public class ResourceControllerProperties : SelectorBuilder<ResourceControllerProperties>
    {
        public static readonly Selector<IImmutableSet<SoftString>> Schemes = Select(() => Schemes);

        public static readonly Selector<IImmutableSet<SoftString>> Tags = Select(() => Tags);

        public static readonly Selector<bool> SupportsRelativeUri = Select(() => SupportsRelativeUri);
    }

    public delegate Task<Stream> CreateStreamCallback(object body);

    public class Request
    {
        [NotNull]
        public UriString Uri { get; set; } = new UriString($"{UriSchemes.Custom.IOnymous}:///");

        [NotNull]
        public RequestMethod Method { get; set; } = RequestMethod.None;

        [NotNull]
        public IImmutableContainer Context { get; set; } = ImmutableContainer.Empty;

        [CanBeNull]
        public CreateStreamCallback CreateBodyStreamCallback { get; set; }

        public object Body { get; set; }

        public Task<Stream> CreateBodyStreamAsync()
        {
            if (Body is null) throw new InvalidOperationException($"Cannot create stream for a null {nameof(Body)}.");
            if (CreateBodyStreamCallback is null) throw new InvalidOperationException($"Cannot create stream without a {nameof(CreateBodyStreamCallback)}.");

            return CreateBodyStreamCallback(Body);
        }

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
    }

    public static class Body
    {
        public static readonly object Null = new object();
    }

    [UseType, UseMember]
    [PlainSelectorFormatter]
    [Rename(nameof(RequestProperty))]
    public class RequestProperty : SelectorBuilder<RequestProperty>
    {
        public static readonly Selector<CancellationToken> CancellationToken = Select(() => CancellationToken);
    }

    public static class RequestExtensions
    {
        public static Request SetProperties(this Request request, Func<IImmutableContainer, IImmutableContainer> properties)
        {
            request.Context = properties(request.Context);
            return request;
        }

        public static Request SetCreateBodyStream(this Request request, CreateStreamCallback createBodyStream)
        {
            request.CreateBodyStreamCallback = createBodyStream;
            return request;
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

    public abstract class ResourceActionAttribute : Attribute
    {
        protected ResourceActionAttribute(RequestMethod method) => Method = method;

        public RequestMethod Method { get; }
    }

    public class ResourceGetAttribute : ResourceActionAttribute
    {
        public ResourceGetAttribute() : base(RequestMethod.Get) { }
    }

    public class ResourcePostAttribute : ResourceActionAttribute
    {
        public ResourcePostAttribute() : base(RequestMethod.Post) { }
    }

    public class ResourcePutAttribute : ResourceActionAttribute
    {
        public ResourcePutAttribute() : base(RequestMethod.Put) { }
    }

    public class ResourceDeleteAttribute : ResourceActionAttribute
    {
        public ResourceDeleteAttribute() : base(RequestMethod.Delete) { }
    }
}