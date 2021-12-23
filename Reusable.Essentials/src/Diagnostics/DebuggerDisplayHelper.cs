using System;
using JetBrains.Annotations;
using Reusable.Essentials.Extensions;

namespace Reusable.Essentials.Diagnostics;

public static class DebuggerDisplayHelper<T>
{
    private static Func<T, string> _toString;

    public static string ToDebuggerDisplayString([CanBeNull] T obj, Action<DebuggerDisplayBuilder<T>> configure)
    {
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        if (_toString is null)
        {
            _toString = new DebuggerDisplayBuilder<T>().Also(configure).Build();
        }

        return _toString(obj);
    }
}

public static class DebuggerDisplayHelper
{
    public static string ToDebuggerDisplayString<T>([CanBeNull] this T obj, Action<DebuggerDisplayBuilder<T>> builderAction)
    {
        if (builderAction == null) throw new ArgumentNullException(nameof(builderAction));

        return DebuggerDisplayHelper<T>.ToDebuggerDisplayString(obj, builderAction);
    }
}