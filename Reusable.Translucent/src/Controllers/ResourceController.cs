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
    public interface IController : IDisposable
    {
        ControllerName Name { get; }

        string? BaseUri { get; }
    }

    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public abstract class Controller : IController
    {
        protected Controller(ControllerName? name, string? baseUri = default)
        {
            Name = name ?? ControllerName.Empty;
            BaseUri = baseUri;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            //builder.DisplayEnumerable(p => p.Properties.Tags(), x => x.ToString());
            //builder.DisplayEnumerable(p => p.Properties.GetSchemes(), x => x.ToString());
            //builder.DisplayValues(p => Names);
            //builder.DisplayValue(x => x.Schemes);
        });

        public ControllerName Name { get; }

        public string? BaseUri { get; }

        // ReSharper disable once InconsistentNaming
        protected static T OK<T>(object? body = default, Action<T>? configure = default) where T : Response, new()
        {
            return new T { StatusCode = ResourceStatusCode.OK, Body = body }.Pipe(configure);
        }

        protected static T NotFound<T>(object? body = default, Action<T>? configure = default) where T : Response, new()
        {
            return new T { StatusCode = ResourceStatusCode.NotFound, Body = body }.Pipe(configure);
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