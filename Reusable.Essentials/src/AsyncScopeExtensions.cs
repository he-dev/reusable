using System.Collections.Generic;

namespace Reusable.Essentials;

public static class AsyncScopeExtensions
{
    public static IEnumerable<AsyncScope<T>> Enumerate<T>(this AsyncScope<T>? scope)
    {
        for (; scope is {}; scope = scope.Parent) yield return scope;
    }
}