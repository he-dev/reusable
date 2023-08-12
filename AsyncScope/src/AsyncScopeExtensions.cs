using System;
using System.Collections.Generic;

namespace Reusable;

public static class AsyncScopeExtensions
{
    public static IEnumerable<AsyncScope<T>> Enumerate<T>(this AsyncScope<T>? scope)
    {
        for (; scope is not null; scope = scope.Parent) yield return scope;
    }
}
