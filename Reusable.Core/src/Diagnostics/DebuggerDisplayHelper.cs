using System;
using JetBrains.Annotations;

namespace Reusable.Diagnostics
{
    public static class DebuggerDisplayHelper<T>
    {
        private static Func<T, string> _toString;

        public static string ToDebuggerDisplayString([CanBeNull] T obj, Action<DebuggerDisplayBuilder<T>> builderAction)
        {
            if (builderAction == null) throw new ArgumentNullException(nameof(builderAction));

            if (_toString is null)
            {
                var builder = new DebuggerDisplayBuilder<T>();
                builderAction(builder);
                _toString = builder.Build();
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