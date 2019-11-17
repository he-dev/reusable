using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.Extensions;
using Reusable.Quickey;

namespace Reusable.Translucent.Controllers
{
    [PublicAPI]
    public interface IResourceController : IDisposable
    {
        [NotNull]
        IImmutableContainer Properties { get; }
    }

    //public delegate Task<Response> InvokeDelegate(Request request);

    [UseType, UseMember]
    [PlainSelectorFormatter]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public abstract class ResourceController : IResourceController
    {
        protected ResourceController([NotNull] IImmutableContainer properties)
        {
            if (properties == null) throw new ArgumentNullException(nameof(properties));

            if (properties.GetItemOrDefault(Schemes) is var schemes && (schemes is null || !schemes.Any()))
            {
                throw new ArgumentException
                (
                    paramName: nameof(properties),
                    message: $"{GetType().ToPrettyString()} must specify at least one scheme."
                );
            }

            Properties = properties.UpdateItem(Tags, tags => tags.Add(GetType().ToPrettyString().ToSoftString()));
        }

        protected ResourceController(IEnumerable<SoftString> schemes, string? baseUri, IImmutableContainer? properties = default)
        {
            Properties =
                properties
                    .ThisOrEmpty()
                    .SetItem(Schemes, ImmutableHashSet<SoftString>.Empty.Union(schemes))
                    .SetItem(BaseUri, baseUri is {} ? new UriString(baseUri) : default);
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            //builder.DisplayEnumerable(p => p.Properties.Tags(), x => x.ToString());
            //builder.DisplayEnumerable(p => p.Properties.GetSchemes(), x => x.ToString());
            //builder.DisplayValues(p => Names);
            //builder.DisplayValue(x => x.Schemes);
        });

        public virtual IImmutableContainer Properties { get; }

        // ReSharper disable once InconsistentNaming
        protected static Response OK(object? body = default, IImmutableContainer? metadata = default) => new Response
        {
            StatusCode = ResourceStatusCode.OK,
            Body = body,
            Metadata = metadata.ThisOrEmpty()
        };

        // ReSharper disable once InconsistentNaming
        //protected static Response OK() => new Response { StatusCode = ResourceStatusCode.OK };

        protected static Response NotFound() => new Response { StatusCode = ResourceStatusCode.NotFound };

        // Can be overriden when derived.
        public virtual void Dispose() { }

        #region Properties

        private static readonly From<ResourceController>? This;

        public static Selector<SoftString?> Id { get; } = This.Select(() => Id);
        
        public static Selector<UriString?> BaseUri { get; } = This.Select(() => BaseUri);

        public static Selector<IImmutableSet<SoftString>> Schemes { get; } = This.Select(() => Schemes);

        public static Selector<IImmutableSet<SoftString>> Tags { get; } = This.Select(() => Tags);

        #endregion
    }

    public static class ImmutableContainerExtensions
    {
        public static IImmutableContainer AddScheme(this IImmutableContainer container, SoftString scheme)
        {
            return container.UpdateItem(ResourceController.Schemes, x => x.Add(scheme));
        }

        public static IImmutableContainer AddTag(this IImmutableContainer container, SoftString tag)
        {
            return container.UpdateItem(ResourceController.Tags, x => x.Add(tag));
        }
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
}