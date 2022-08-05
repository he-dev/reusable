using System;

namespace Reusable.Marbles.Extensions;

public static class DisposableExtensions
{
    public static IDisposable Attach(this IDisposable first, IDisposable second) => Disposable.Create(() =>
    {
        first.Dispose();
        second.Dispose();
    });
}