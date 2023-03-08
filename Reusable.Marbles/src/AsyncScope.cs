using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Reusable.Marbles;

public class AsyncScope<T> : IDisposable
{
    private static readonly AsyncLocal<AsyncScope<T>> State = new();

    private AsyncScope(T value) => Value = value;

    public T Value { get; }

    public AsyncScope<T>? Parent { get; private init; }

    public static AsyncScope<T>? Current => State.Value;

    /// <summary>
    /// Enumerates scopes. Deepest first.
    /// </summary>
    public static IEnumerable<T> Enumerate() => Current?.Enumerate().Select(s => s.Value) ?? Enumerable.Empty<T>();

    /// <summary>
    /// Gets a value indicating whether there are any states on the stack.
    /// </summary>
    public static bool Exists => State.Value is { };

    public static AsyncScope<T> Push(T value)
    {
        return State.Value = new AsyncScope<T>(value)
        {
            Parent = Current,
        };
    }

    public void Dispose()
    {
        //(State.Value!.Value as IDisposable)?.Dispose();
        State.Value = State.Value!.Parent;
    }

    public static implicit operator T(AsyncScope<T> scope) => scope.Value;
}

public static class AsyncScope
{
    public static AsyncScope<T> Push<T>(T item) => AsyncScope<T>.Push(item);
}