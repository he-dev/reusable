using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Reusable.Marbles;
using Reusable.Marbles.Extensions;

namespace Reusable.Synergy;

public interface IRequest : IDisposable
{
    IDictionary<string, object> Items { get; }

    CancellationToken CancellationToken { get; set; }
}

public interface IRequest<T> : IRequest { }

public abstract class Request<T> : IRequest<T>
{
    public IDictionary<string, object> Items { get; } = new Dictionary<string, object>(SoftString.Comparer);

    public CancellationToken CancellationToken { get; set; } = CancellationToken.None;

    public virtual void Dispose() { }
}

public sealed class Unit
{
    private Unit() { }

    public static readonly Unit Default = new();
}

public interface IService : IEnumerable<IService>, IDisposable
{
    bool MustSucceed { get; }

    // Points to the next middleware.
    IService? Next { get; set; }

    Task<object> InvokeAsync(IRequest request);
}

public abstract class Service : IService
{
    public bool MustSucceed { get; set; }

    public IService? Next { get; set; }

    public abstract Task<object> InvokeAsync(IRequest request);

    public IEnumerator<IService> GetEnumerator() => this.Enumerate().GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    protected async Task<object> InvokeNext(IRequest request)
    {
        return
            Next is { } next
                ? await next.InvokeAsync(request)
                : Unit.Default;
    }

    [DoesNotReturn]
    protected static object Failure<T>(T request, string message) where T : IRequest
    {
        throw DynamicException.Create(request.GetType().ToPrettyString(), message);
    }

    protected T ThrowIfNot<T>(IRequest request) where T : IRequest
    {
        if (request is not T result)
        {
            throw DynamicException.Create("Request", $"{GetType().ToPrettyString()} does not support {request.GetType().ToPrettyString()}. You need to use {request.GetType().ToPrettyString()}");
        }

        return result;
    }

    protected class Empty : Service
    {
        public override async Task<object> InvokeAsync(IRequest request) => await InvokeNext(request);
    }

    public class PipelineBuilder : IEnumerable<IService>
    {
        private IService First { get; } = new Empty();

        // Adds the specified service at the end of the pipeline.
        public void Add(IService last) => First.Enumerate().Last().Next = last;

        public IEnumerator<IService> GetEnumerator() => First.Enumerate().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IService Build() => First;
    }

    public virtual void Dispose() { }
}