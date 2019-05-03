using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Flexo
{
    public static class ExpressionExtensions
    {
        /// <summary>
        /// Gets only enabled expressions.
        /// </summary>
        public static IEnumerable<IExpression> Enabled(this IEnumerable<IExpression> expressions)
        {
            return
                from expression in expressions
                where expression.Enabled
                select expression;
        }

        public static IEnumerable<IConstant> Invoke(this IEnumerable<IExpression> expressions)
        {
            return
                from expression in expressions.Enabled()
                select expression.Invoke();
        }

        public static IEnumerable<T> Values<T>(this IEnumerable<IConstant> constants)
        {
            return
                from constant in constants
                select constant.Value<T>();
        }

        /// <summary>
        /// Gets the value of a Constant expression if it's of the specified type T or throws an exception.
        /// </summary>
        public static T Value<T>(this IConstant constant)
        {
            if (typeof(T) == typeof(object))
            {
                return (T)constant.Value;
            }
            else
            {
                return
                    constant.Value is T value
                        ? value
                        : throw DynamicException.Create
                        (
                            "ValueType",
                            $"Expected {typeof(Constant<T>).ToPrettyString()} but found {constant.GetType().ToPrettyString()}."
                        );
            }
        }

        public static T ValueOrDefault<T>(this IConstant expression)
        {
            return
                expression is IConstant constant && constant.Value is T value
                    ? value
                    : default;
        }

        public static object ValueOrDefault(this IConstant expression)
        {
            return
                expression is IConstant constant
                    ? constant.Value
                    : default;
        }
    }
}