using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Diagnostics;
using Reusable.Extensions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Abstractions
{
    [PublicAPI]
    public interface IResourceController : IDisposable
    {
        ControllerName Name { get; }

        string? BaseUri { get; }

        Type RequestType { get; }

        Task<Response> InvokeAsync(Request request);

        Task<Response> CreateAsync(Request request);

        Task<Response> ReadAsync(Request request);

        Task<Response> UpdateAsync(Request request);

        Task<Response> DeleteAsync(Request request);
    }

    [DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
    public abstract class ResourceController<T> : IResourceController where T : Request
    {
        protected ResourceController(ControllerName? name, string? baseUri = default)
        {
            Name = name ?? ControllerName.Any;
            BaseUri = baseUri;
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplaySingle(c => c.Name);
            builder.DisplaySingle(c => c.BaseUri);
            builder.DisplaySingle(c => c.RequestType, t => t.ToPrettyString());
        });

        public ControllerName Name { get; }

        public string? BaseUri { get; }

        public Type RequestType => typeof(T);

        public Task<Response> InvokeAsync(Request request)
        {
            return request.Method switch
            {
                ResourceMethod.Create => CreateAsync(request as T),
                ResourceMethod.Read => ReadAsync(request as T),
                ResourceMethod.Update => UpdateAsync(request as T),
                ResourceMethod.Delete => DeleteAsync(request as T),
                ResourceMethod.None => throw new InvalidOperationException("You must specify a request method."),
                _ => throw new NotSupportedException($"Request method '{request.Method}' is not supported.")
            };
        }

        Task<Response> IResourceController.CreateAsync(Request request) => CreateAsync(request as T);

        Task<Response> IResourceController.ReadAsync(Request request) => ReadAsync(request as T);

        Task<Response> IResourceController.UpdateAsync(Request request) => UpdateAsync(request as T);

        Task<Response> IResourceController.DeleteAsync(Request request) => DeleteAsync(request as T);

        public virtual Task<Response> CreateAsync(T request) => throw NotSupportedException();

        public virtual Task<Response> ReadAsync(T request) => throw NotSupportedException();

        public virtual Task<Response> UpdateAsync(T request) => throw NotSupportedException();

        public virtual Task<Response> DeleteAsync(T request) => throw NotSupportedException();

        private Exception NotSupportedException([CallerMemberName] string? name = default)
        {
            throw new NotSupportedException($"{GetType().ToPrettyString()} ({Name}) does not support '{name}'.");
        }

        // ReSharper disable once InconsistentNaming
        protected static TResponse Success<TResponse>(object? body = default, Action<TResponse>? configure = default) where TResponse : Response, new()
        {
            return new TResponse { StatusCode = ResourceStatusCode.Success, Body = body }.Pipe(configure);
        }

        protected static TResponse NotFound<TResponse>(object? body = default, Action<TResponse>? configure = default) where TResponse : Response, new()
        {
            return new TResponse { StatusCode = ResourceStatusCode.NotFound, Body = body }.Pipe(configure);
        }

        // Can be overriden when derived.
        public virtual void Dispose() { }
    }
}