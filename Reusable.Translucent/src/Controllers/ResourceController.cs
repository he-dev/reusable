using System;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
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

    public delegate Task<Response> InvokeDelegate(Request request);

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
                    message: $"{GetType().ToPrettyString().ToSoftString()} must specify at least one scheme."
                );
            }

            Properties = properties.UpdateItem(Tags, tags => tags.Add(GetType().ToPrettyString().ToSoftString()));
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
        protected Response OK(object body, IImmutableContainer metadata = default) => new Response
        {
            StatusCode = ResourceStatusCode.OK,
            Body = body,
            Metadata = metadata
        };

        // ReSharper disable once InconsistentNaming
        protected Response OK() => new Response { StatusCode = ResourceStatusCode.OK };

        protected Response NotFound() => new Response { StatusCode = ResourceStatusCode.NotFound };

        // Can be overriden when derived.
        public virtual void Dispose() { }

        #region Properties

        private static readonly From<ResourceController> This;

        public static readonly Selector<IImmutableSet<SoftString>> Schemes = This.Select(() => Schemes);

        public static readonly Selector<IImmutableSet<SoftString>> Tags = This.Select(() => Tags);

        public static readonly Selector<bool> SupportsRelativeUri = This.Select(() => SupportsRelativeUri);

        #endregion
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