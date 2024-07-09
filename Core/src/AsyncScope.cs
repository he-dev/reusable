using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Reusable;

public class AsyncScope<T> : IDisposable, IEnumerable<AsyncScope<T>>
{
    private static readonly AsyncLocal<AsyncScope<T>> State = new();

    private AsyncScope(T value) => Value = value;

    public T Value { get; }

    public AsyncScope<T>? Parent { get; private init; }

    public static AsyncScope<T>? Current => State.Value;

    /// <summary>
    /// Enumerates scopes. Deepest first.
    /// </summary>
    public static IEnumerable<T> Enumerate() => Current?.Select(s => s.Value) ?? [];

    /// <summary>
    /// Gets a value indicating whether there are any states on the stack.
    /// </summary>
    public static bool Exists => State.Value is not null;

    public static AsyncScope<T> Push(T value)
    {
        return State.Value = new AsyncScope<T>(value)
        {
            Parent = Current,
        };
    }

    public void Dispose()
    {
        State.Value = State.Value!.Parent;
    }

    public static implicit operator T(AsyncScope<T> scope) => scope.Value;

    public IEnumerator<AsyncScope<T>> GetEnumerator()
    {
        for (var scope = this; scope is not null; scope = scope.Parent)
        {
            yield return scope;
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

public static class AsyncScope
{
    //public static AsyncScope<T> Push<T>(T item) => AsyncScope<T>.Push(item);

    public static Action Push<T>(T item)
    {
        var scope = AsyncScope<T>.Push(item);
        return () => scope.Dispose();
    }
}