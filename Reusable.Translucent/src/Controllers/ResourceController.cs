using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Extensions;

namespace Reusable.Translucent.Controllers
{
    [PublicAPI]
    public interface IResourceController : IDisposable
    {
        SoftString? Id { get; }

        UriString? BaseUri { get; }

        /// <summary>
        /// Gets a collection of supported schemes. 
        /// </summary>
        ISet<SoftString> Schemes { get; }

        /// <summary>
        /// Gets a collection of tags that can be used to address a set of controllers.
        /// </summary>
        ISet<SoftString> Tags { get; }
        
        bool SupportsRelativeUri { get; }
    }

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

        public ISet<SoftString> Tags { get; set; } = new HashSet<SoftString>();

        public bool SupportsRelativeUri => BaseUri is {};

        // ReSharper disable once InconsistentNaming
        protected static Response OK<T>(object? body = default, Action<T>? responseAction = default) where T : Response, new()
        {
            return new T { StatusCode = ResourceStatusCode.OK, Body = body }.Pipe(responseAction);
        }

        protected static Response NotFound<T>(object? body = default, Action<T>? responseAction = default) where T : Response, new()
        {
            return new T { StatusCode = ResourceStatusCode.NotFound, Body = body }.Pipe(responseAction);
        }

        // Can be overriden when derived.
        public virtual void Dispose() { }
    }

    /// <summary>
    /// Specifies which types of request a controller can handle.
    /// </summary>
    public class HandlesAttribute : Attribute
    {
        public HandlesAttribute(Type type) => Type = type;
        
        public Type Type { get; }
    }
}