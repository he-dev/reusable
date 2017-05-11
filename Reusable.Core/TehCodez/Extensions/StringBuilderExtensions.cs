using System;
using System.Text;

namespace Reusable.Extensions
{
    public static class StringBuilderExtensions
    {
        public static StringBuilder AppendWhen(this StringBuilder @this, Func<bool> predicate, Func<StringBuilder, StringBuilder> append)
        {
            return predicate() ? append(@this) : @this;
        }

        public static StringBuilder AppendWhen<T>(this StringBuilder @this, Func<T> getValue, Func<T, bool> predicate, Func<StringBuilder, T, StringBuilder> append)
        {
            var value = getValue();
            return predicate(value) ? append(@this, value) : @this;
        }
    }
}
