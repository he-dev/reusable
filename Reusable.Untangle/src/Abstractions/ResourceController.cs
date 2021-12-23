using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Diagnostics;
using Reusable.Essentials.Extensions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Abstractions;

using static RequestMethod;

[PublicAPI]
public interface IResourceController : IDisposable
{
    string Name { get; }

    ISet<string> Tags { get; }

    string? ResourceNameRoot { get; }

    Type RequestType { get; }

    Task<Response> InvokeAsync(Request request);

    Task<Response> CreateAsync(Request request);

    Task<Response> ReadAsync(Request request);

    Task<Response> UpdateAsync(Request request);

    Task<Response> DeleteAsync(Request request);
}

[DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
public abstract class ResourceController<TRequest> : IResourceController where TRequest : Request
{
    private string DebuggerDisplay => this.ToDebuggerDisplayString(builder =>
    {
        builder.DisplaySingle(c => c.Name);
        builder.DisplayEnumerable(c => c.Tags);
        builder.DisplaySingle(c => c.ResourceNameRoot);
        builder.DisplaySingle(c => c.RequestType, t => t.ToPrettyString());
    });

    public string Name { get; set; } = default!;

    public ISet<string> Tags { get; } = new SortedSet<string>(SoftString.Comparer);

    public virtual string? ResourceNameRoot { get; set; }

    public Type RequestType => typeof(TRequest);

    public Task<Response> InvokeAsync(Request request)
    {
        if (request is TRequest r)
        {
            return request.Method switch
            {
                Create => CreateAsync(r),
                Read => ReadAsync(r),
                Update => UpdateAsync(r),
                Delete => DeleteAsync(r),
                None => throw new InvalidOperationException("You must specify a request method."),
                _ => throw new NotSupportedException($"Request method '{request.Method}' is not supported.")
            };
        }
        else
        {
            throw new ArgumentException
            (
                $"{GetType().ToPrettyString()} cannot process the request '{request.ResourceName}' " +
                $"because it must by of type '{typeof(TRequest).ToPrettyString()}' but was '{request.GetType().ToPrettyString()}'."
            );
        }
    }

    Task<Response> IResourceController.CreateAsync(Request request) => InvokeAsync(request);

    Task<Response> IResourceController.ReadAsync(Request request) => InvokeAsync(request);

    Task<Response> IResourceController.UpdateAsync(Request request) => InvokeAsync(request);

    Task<Response> IResourceController.DeleteAsync(Request request) => InvokeAsync(request);

    public virtual Task<Response> CreateAsync(TRequest request) => throw NotSupportedException();

    public virtual Task<Response> ReadAsync(TRequest request) => throw NotSupportedException();

    public virtual Task<Response> UpdateAsync(TRequest request) => throw NotSupportedException();

    public virtual Task<Response> DeleteAsync(TRequest request) => throw NotSupportedException();

    protected Exception NotSupportedException([CallerMemberName] string? name = default)
    {
        throw new NotSupportedException($"{GetType().ToPrettyString()} ({Name}) does not support '{name!.ToUpper()}'.");
    }

    protected static Response Success<TResponse>(Stack<string> resourceName, object? body = default, Action<TResponse>? configure = default) where TResponse : Response, new()
    {
        return new TResponse { ResourceName = resourceName.Peek(), StatusCode = ResourceStatusCode.Success, Body = { body } }.Also(configure);
    }

    protected static Response NotFound<TResponse>(Stack<string> resourceName, object? body = default, Action<TResponse>? configure = default) where TResponse : Response, new()
    {
        return new TResponse { ResourceName = resourceName.Peek(), StatusCode = ResourceStatusCode.NotFound, Body = { body } }.Also(configure);
    }

    // Can be overriden when derived.
    public virtual void Dispose() { }
}