using System;
using System.Collections.Generic;

namespace Reusable.Marbles;

public static class AsyncScopeExtensions
{
    public static IEnumerable<AsyncScope<T>> Enumerate<T>(this AsyncScope<T>? scope) where T : IDisposable
    {
        for (; scope is {}; scope = scope.Parent) yield return scope;
    }
}