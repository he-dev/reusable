using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Flexo
{
    public static class ExpressionExtensions
    {
        public static IEnumerable<IExpression> Enabled(this IEnumerable<IExpression> expressions)
        {
            return
                from expression in expressions
                where expression.Enabled
                select expression;
        }

        public static IEnumerable<T> Values<T>(this IEnumerable<IExpression> expressions)
        {
            return
                from expression in expressions
                select expression.Value<T>();
        }

        /// <summary>
        /// Gets the value of a Constant expression or throws an exception if not a Constant.
        /// </summary>
        public static object Value(this IExpression expression)
        {
            return
                expression is IConstant constant
                    ? constant.Value
                    : throw new InvalidExpressionException(typeof(IConstant), expression.GetType());
        }

        /// <summary>
        /// Gets the value of a Constant expression if it's of the specified type T or throws an exception.
        /// </summary>
        public static T Value<T>(this IExpression expression)
        {
            if (typeof(T) == typeof(object))
            {
                return (T)expression.Value();
            }
            else
            {
                return
                    expression is Constant<T> constant
                        ? constant.Value
                        : throw new InvalidExpressionException(typeof(Constant<T>), expression.GetType());
            }
        }

        public static T ValueOrDefault<T>(this IExpression expression)
        {
            return
                expression is Constant<T> constant
                    ? constant.Value
                    : default;
        }

        public static object ValueOrDefault(this IExpression expression)
        {
            return
                expression is IConstant constant
                    ? constant.Value
                    : default;
        }

        [CanBeNull]
        public static IExpression Input([NotNull] this IExpressionContext context)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            return context.Get(Item.For<IExtensionContext>(), x => x.Input);
        }
    }
}