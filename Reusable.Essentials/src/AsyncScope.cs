using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;

namespace Reusable.Essentials;

public class AsyncScope<T> : IDisposable where T : IDisposable
{
    private static readonly AsyncLocal<AsyncScope<T>> State = new();

    private AsyncScope(T value) => Value = value;

    public T Value { get; }

    public AsyncScope<T>? Parent { get; private init; }

    public static AsyncScope<T>? Current => State.Value;

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

    public static AsyncScope<T> Push(Func<IDisposable, T> create)
    {
        var value = create(Disposable.Create(() => State.Value?.Dispose()));

        return State.Value = new AsyncScope<T>(value)
        {
            Parent = State.Value,
        };
    }

    public void Dispose()
    {
        if (State.Value?.Parent is { } parent)
        {
            State.Value = parent;
        }
    }

    public static implicit operator T(AsyncScope<T> scope) => scope.Value;
}