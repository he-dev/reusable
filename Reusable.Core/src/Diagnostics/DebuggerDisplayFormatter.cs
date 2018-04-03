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
        public const string DefaultValueFormat = "{0}";

        public const int DefaultCollectionLength = 10;

        public static string FormatValue<TValue>([CanBeNull] this TValue value, [NotNull] string format)
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
            return value.FormatValue(DefaultValueFormat);
        }

        public static string FormatCollection<TValue>([CanBeNull] this IEnumerable<TValue> values, [NotNull] string format, int max)
        {
            if (format == null) throw new ArgumentNullException(nameof(format));

            if (values == null) return "null";

            // [1, 2, 3, ...] (max = 10)
            return $"[{string.Join(", ", values.Select(x => x.FormatValue(format)).Take(max))}, ...] (max {max})"; 
        }

        // Foo.Bar(..).Baz
        public static string FormatMemberName([NotNull] this IEnumerable<Expression> expressions)
        {
            if (expressions == null) throw new ArgumentNullException(nameof(expressions));

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