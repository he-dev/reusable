﻿using System;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Diagnostics
{
    public static class DebuggerDisplayHelper<T>
    {
        private static Func<T, string> _toString;

        public static string ToDebuggerDisplayString([CanBeNull] T obj, Action<DebuggerDisplayBuilder<T>> configure)
        {
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            if (_toString is null)
            {
                _toString = new DebuggerDisplayBuilder<T>().Pipe(configure).Build();
            }

            return _toString(obj);
        }
    }

    public static class DebuggerDisplayHelper
    {
        public static string ToDebuggerDisplayString<T>([CanBeNull] this T obj, [NotNull] Action<DebuggerDisplayBuilder<T>> builderAction)
        {
            if (builderAction == null) throw new ArgumentNullException(nameof(builderAction));

            return DebuggerDisplayHelper<T>.ToDebuggerDisplayString(obj, builderAction);
        }
    }
}