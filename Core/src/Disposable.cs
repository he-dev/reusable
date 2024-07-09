using System;

namespace Reusable;

public class Disposable(Action dispose) : IDisposable
{
    public static readonly IDisposable Empty = new Disposable(() => { });

    private Action DisposeFunc { get; } = dispose;

    public static IDisposable From(Action dispose) => new Disposable(dispose);

    public void Dispose() => DisposeFunc();
}