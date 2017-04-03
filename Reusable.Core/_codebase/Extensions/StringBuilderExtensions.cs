using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Tests.Extensions
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
