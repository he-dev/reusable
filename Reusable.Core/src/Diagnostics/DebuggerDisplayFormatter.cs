using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Reusable.Diagnostics
{
    internal static class DebuggerDisplayFormatter
    {
        public const string DefaultFormatString = "{0}";

        public const int DefaultEnumerableCount = 10;

        public static string FormatValue<TValue>([CanBeNull] this TValue value, string format)
        {
            if (format == null) throw new ArgumentNullException(nameof(format));

            if (value == null) return "null";
            if (value is DBNull) return $"{nameof(DBNull)}";

            var valueFormatted = string.Format(CultureInfo.InvariantCulture, format, value);

            return
                value is string
                    ? $"'{valueFormatted}'"
                    : valueFormatted;
        }

        public static string FormatValue<TValue>([CanBeNull] this TValue value)
        {
            return value.FormatValue(DefaultFormatString);
        }

        public static string FormatEnumerable<TValue>(this IEnumerable<TValue>? values, string format, int max)
        {
            // [1, 2, 3, ...] (max = 10)
            return
                values == null
                    ? "null"
                    : $"[{string.Join(", ", values.Select(x => x.FormatValue(format)).Take(max))}, ...]";
        }

        // Foo.Bar(..).Baz
        public static string FormatMemberName(this IEnumerable<Expression> expressions)
        {
            return string.Join(".", expressions.GetMemberNames());
        }

        private static IEnumerable<string> GetMemberNames([NotNull] this IEnumerable<Expression> expressions)
        {
            if (expressions == null) throw new ArgumentNullException(nameof(expressions));

            foreach (var expression in expressions)
            {
                switch (expression)
                {
                    case MemberExpression memberExpression:
                        yield return memberExpression.Member.Name;
                        break;
                    case MethodCallExpression methodCallExpression:
                        // Ignore ToString calls.
                        if (methodCallExpression.Method.Name == nameof(ToString)) continue;
                        yield return $"{methodCallExpression.Method.Name}(..)";
                        break;
                }
            }
        }
    }
}