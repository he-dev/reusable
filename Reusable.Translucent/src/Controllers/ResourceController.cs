using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Extensions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Controllers
{
    [PublicAPI]
    public interface IResourceController : IDisposable
    {
        ControllerName ControllerName { get; }

        UriString? BaseUri { get; }

        /// <summary>
        /// Gets a collection of supported schemes. 
        /// </summary>
        ISet<SoftString> Schemes { get; }

        bool SupportsRelativeUri { get; }
    }

    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public abstract class ResourceController : IResourceController
    {
        protected ResourceController(ControllerName controllerName, string? basePath, params SoftString[] schemes)
        {
            ControllerName = controllerName;
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

        public ControllerName ControllerName { get; }

        public UriString? BaseUri { get; }

        public ISet<SoftString> Schemes { get; }

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