using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Diagnostics;
using Reusable.Quickey;

namespace Reusable.Translucent.Controllers
{
    [PublicAPI]
    public interface IResourceController : IDisposable
    {
        SoftString? Id { get; }

        UriString? BaseUri { get; }

        ISet<SoftString> Schemes { get; }

        ISet<SoftString> Tags { get; }

        bool SupportsRelativeUri { get; }
    }

    [UseType, UseMember]
    [PlainSelectorFormatter]
    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public abstract class ResourceController : IResourceController
    {
        protected ResourceController(string? id, string? basePath, params SoftString[] schemes)
        {
            Id = id;
            BaseUri = basePath is {} uri ? new UriString(uri) : default;
            Schemes = new HashSet<SoftString>(schemes);
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            //builder.DisplayEnumerable(p => p.Properties.Tags(), x => x.ToString());
            //builder.DisplayEnumerable(p => p.Properties.GetSchemes(), x => x.ToString());
            //builder.DisplayValues(p => Names);
            //builder.DisplayValue(x => x.Schemes);
        });

        public SoftString? Id { get; }

        public UriString? BaseUri { get; }

        public ISet<SoftString> Schemes { get; }

        public ISet<SoftString> Tags { get; set; }
        
        public bool SupportsRelativeUri => BaseUri is {};

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