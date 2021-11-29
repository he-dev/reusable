using System;
using System.Collections.Generic;
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
        SoftString? Name { get; }

        ISet<SoftString> Tags { get; }

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
        private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
        {
            builder.DisplaySingle(c => c.Name);
            builder.DisplayEnumerable(c => c.Tags);
            builder.DisplaySingle(c => c.BaseUri);
            builder.DisplaySingle(c => c.RequestType, t => t.ToPrettyString());
        });

        public SoftString? Name { get; set; }

        public ISet<SoftString> Tags { get; } = new SortedSet<SoftString>();

        public virtual string? BaseUri { get; }

        public Type RequestType => typeof(T);

        public Task<Response> InvokeAsync(Request request)
        {
            if (request is T r)
            {
                return request.Method switch
                {
                    ResourceMethod.Create => CreateAsync(r),
                    ResourceMethod.Read => ReadAsync(r),
                    ResourceMethod.Update => UpdateAsync(r),
                    ResourceMethod.Delete => DeleteAsync(r),
                    ResourceMethod.None => throw new InvalidOperationException("You must specify a request method."),
                    _ => throw new NotSupportedException($"Request method '{request.Method}' is not supported.")
                };
            }
            else
            {
                throw new ArgumentException
                (
                    $"{GetType().ToPrettyString()} cannot process the request '{request.ResourceName}' " +
                    $"because it must by of type '{typeof(T).ToPrettyString()}' but was '{request.GetType().ToPrettyString()}'."
                );
            }
        }

        Task<Response> IResourceController.CreateAsync(Request request) => InvokeAsync(request);

        Task<Response> IResourceController.ReadAsync(Request request) => InvokeAsync(request);

        Task<Response> IResourceController.UpdateAsync(Request request) => InvokeAsync(request);

        Task<Response> IResourceController.DeleteAsync(Request request) => InvokeAsync(request);

        public virtual Task<Response> CreateAsync(T request) => throw NotSupportedException();

        public virtual Task<Response> ReadAsync(T request) => throw NotSupportedException();

        public virtual Task<Response> UpdateAsync(T request) => throw NotSupportedException();

        public virtual Task<Response> DeleteAsync(T request) => throw NotSupportedException();

        protected Exception NotSupportedException([CallerMemberName] string? name = default)
        {
            throw new NotSupportedException($"{GetType().ToPrettyString()} ({Name}) does not support '{name!.ToUpper()}'.");
        }

        // ReSharper disable once InconsistentNaming
        protected static Response Success<TResponse>(string resourceName, object? body = default, Action<TResponse>? configure = default) where TResponse : Response, new()
        {
            return new TResponse { ResourceName = resourceName, StatusCode = ResourceStatusCode.Success, Body = body }.Also(configure);
        }

        protected static Response NotFound<TResponse>(string resourceName, object? body = default, Action<TResponse>? configure = default) where TResponse : Response, new()
        {
            return new TResponse { ResourceName = resourceName, StatusCode = ResourceStatusCode.NotFound, Body = body }.Also(configure);
        }

        // Can be overriden when derived.
        public virtual void Dispose() { }
    }
}