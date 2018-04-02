using System;

namespace Reusable.Diagnostics
{
    public static class DebuggerDisplayHelper<T>
    {
        private static Func<T, string> _toString;

        public static string ToString(T obj, Action<DebuggerDisplayBuilder<T>> builderAction)
        {
            if (_toString is null)
            {
                var builder = new DebuggerDisplayBuilder<T>();
                builderAction(builder);
                _toString = builder.Build();
            }

            return _toString(obj);
        }
    }
}