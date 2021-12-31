using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Essentials;
using Reusable.Essentials.Diagnostics;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Data;

namespace Reusable.Octopus.Abstractions;

using static RequestMethod;

[PublicAPI]
public interface IResourceController : IDisposable
{
    ISet<string> Schema { get; }

    string Name { get; }

    ISet<string> Tags { get; }

    string? ResourceNameRoot { get; }

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
        builder.DisplaySingle(c => c.ResourceNameRoot);
        //builder.DisplaySingle(c => c.RequestType, t => t.ToPrettyString());
    });

    protected ResourceController(IEnumerable<string> schema) => Schema = schema.ToHashSet(SoftString.Comparer);

    public ISet<string> Schema { get; }

    public string Name { get; set; } = default!;

    public ISet<string> Tags { get; } = new SortedSet<string>(SoftString.Comparer);

    public virtual string? ResourceNameRoot { get; set; }

    public Task<Response> InvokeAsync(Request request)
    {
        if (!Schema.Contains(request.Schema))
        {
            throw DynamicException.Create("Schema", $"Controller '{GetType().ToPrettyString()}' expects such schemas as [{Schema.Join(", ")}] but received '{request.Schema}'.");
        }

        if (request is not T r)
        {
            throw DynamicException.Create("Request", $"{GetType().ToPrettyString()} expects requests of type '{typeof(T).ToPrettyString()}' but received '{request.GetType().ToPrettyString()}'.");
        }

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

    Task<Response> IResourceController.CreateAsync(Request request) => InvokeAsync(request);

    Task<Response> IResourceController.ReadAsync(Request request) => InvokeAsync(request);

    Task<Response> IResourceController.UpdateAsync(Request request) => InvokeAsync(request);

    Task<Response> IResourceController.DeleteAsync(Request request) => InvokeAsync(request);

    protected virtual Task<Response> CreateAsync(T request) => throw NotSupportedException();

    protected virtual Task<Response> ReadAsync(T request) => throw NotSupportedException();

    protected virtual Task<Response> UpdateAsync(T request) => throw NotSupportedException();

    protected virtual Task<Response> DeleteAsync(T request) => throw NotSupportedException();

    protected Exception NotSupportedException([CallerMemberName] string name = "")
    {
        throw new NotSupportedException($"{GetType().ToPrettyString()} ({Name}) does not support '{name.ToUpper()}'.");
    }

    protected static Response Success<TResponse>(string resourceName, object? body = default, Action<TResponse>? configure = default) where TResponse : Response, new()
    {
        return new TResponse
        {
            ResourceName = resourceName,
            StatusCode = ResourceStatusCode.Success,
            Body = { body }
        }.Also(configure);
    }

    protected static Response NotFound<TResponse>(string resourceName, object? body = default, Action<TResponse>? configure = default) where TResponse : Response, new()
    {
        return new TResponse
        {
            ResourceName = resourceName,
            StatusCode = ResourceStatusCode.NotFound,
            Body = { body }
        }.Also(configure);
    }

    // Can be overriden when derived.
    public virtual void Dispose() { }
}