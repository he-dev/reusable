using System;
using System.Threading;

namespace Reusable.Essentials;

public class AsyncScope<T> : IDisposable where T : IDisposable
{
    private static readonly AsyncLocal<AsyncScope<T>> State = new();

    private AsyncScope(T value) => Value = value;

    public T Value { get; }

    public AsyncScope<T>? Parent { get; private init; }

    public static AsyncScope<T> Current
    {
        get => State.Value ?? throw new InvalidOperationException($"There is no current scope.");
        private set => State.Value = value!;
    }

    /// <summary>
    /// Gets a value indicating whether there are any states on the stack.
    /// </summary>
    public static bool Exists => State.Value is { };

    public static AsyncScope<T> Push(T value)
    {
        return Current = new AsyncScope<T>(value)
        {
            Parent = Current,
        };
    }

    public static AsyncScope<T> Push(Func<IDisposable, T> create)
    {
        var value = create(Disposable.Create(() => Current.Dispose()));

        return Current = new AsyncScope<T>(value)
        {
            Parent = Current,
        };
    }

    public void Dispose()
    {
        if (Current.Parent is { } parent)
        {
            Current = parent;
        }
    }

    public static implicit operator T(AsyncScope<T> scope) => scope.Value;
}

// public interface IAsyncScopeItem : IDisposable
// {
//     Action Disposer { get; set; }
// }