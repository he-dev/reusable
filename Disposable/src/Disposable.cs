using System;
using JetBrains.Annotations;

namespace Reusable;

[PublicAPI]
public class Disposable : IDisposable
{
    private readonly Action _dispose;

    private Disposable(Action dispose) => _dispose = dispose;

    public static IDisposable From(Action dispose) => new Disposable(dispose);

    public void Dispose() => _dispose();

    public class Empty : Disposable
    {
        public Empty() : base(() => { }) { }
    }
}