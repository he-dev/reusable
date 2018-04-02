using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Reusable.Diagnostics
{
    internal static class DebuggerDisplayFormatter
    {
        public static string FormatValue<TValue>(this TValue value)
        {
            if (Type.GetTypeCode(value?.GetType()) == DBNull.Value.GetTypeCode()) return $"{nameof(DBNull)}";
            if (value == null) return "null";
            if (value.IsNumeric()) return value.ToString();

            return $"'{value}'";
        }

        public static string FormatCollection<TValue>(this IEnumerable<TValue> values)
        {
            if (values == null) return "null";

            return "[" + string.Join(", ", values.Select(FormatValue)) + "]";
        }

        public static string FormatMemberName(this IEnumerable<Expression> expressions)
        {
            return string.Join(".", expressions.GetMemberNames());
        }

        private static IEnumerable<string> GetMemberNames(this IEnumerable<Expression> expressions)
        {
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

        private static readonly ISet<TypeCode> NumericTypeCodes = new HashSet<TypeCode>
        {
            TypeCode.Byte,
            TypeCode.SByte,
            TypeCode.UInt16,
            TypeCode.UInt32,
            TypeCode.UInt64,
            TypeCode.Int16,
            TypeCode.Int32,
            TypeCode.Int64,
            TypeCode.Decimal,
            TypeCode.Double,
            TypeCode.Single,
        };

        public static bool IsNumeric<TValue>(this TValue value)
        {
            return NumericTypeCodes.Contains(Type.GetTypeCode(typeof(TValue)));
        }
    }
}