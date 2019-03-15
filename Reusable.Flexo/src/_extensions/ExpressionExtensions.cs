using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Reusable.Flexo.Diagnostics;

namespace Reusable.Flexo
{
    public static class ExpressionExtensions
    {
        public static TExpression ValidateInItems<TExpression>(this TExpression expression, IExpressionContext context) where TExpression : IExpression
        {
            var missingInParameters =
                typeof(TExpression)
                    .GetCustomAttributes<OutAttribute>()
                    .Where(parameter => parameter.Required && !context.ContainsKey(parameter.Name))
                    .ToList();

            return
                missingInParameters.Any()
                    ? throw new MissingInParameterException(missingInParameters)
                    : expression;
        }

        public static IEnumerable<TExpression> ValidateInItems<TExpression>(this IEnumerable<TExpression> expressions, IExpressionContext context) where TExpression : IExpression
        {
            return expressions.Select(expression => expression.ValidateItems<TExpression, OutAttribute>(context));
        }

        public static TExpression ValidateOutItems<TExpression>(this TExpression expression, IExpressionContext context) where TExpression : IExpression
        {
            return expression.ValidateItems<TExpression, InAttribute>(context);
        }

        public static IEnumerable<TExpression> ValidateOutItems<TExpression>(this IEnumerable<TExpression> expressions, IExpressionContext context) where TExpression : IExpression
        {
            return expressions.Select(expression => expression.ValidateItems<TExpression, InAttribute>(context));
        }

        public static TExpression ValidateItems<TExpression, TParameterAttribute>(this TExpression expression, IExpressionContext context) where TExpression : IExpression where TParameterAttribute : Attribute, IParameterAttribute
        {
            var missingItems =
                typeof(TExpression)
                    .GetCustomAttributes<TParameterAttribute>()
                    .Where(item => item.Required && !context.ContainsKey(item.Name))
                    .ToList();

            if (missingItems.Any())
            {
                if (typeof(TParameterAttribute) == typeof(InAttribute)) throw new MissingOutParameterException(missingItems);
                if (typeof(TParameterAttribute) == typeof(OutAttribute)) throw new MissingInParameterException(missingItems);
                throw new ArgumentException($"Inalid parameter attribute: {typeof(TParameterAttribute).FullName}");
            }

            return expression;
        }

        public static IExpression InvokeWithValidation(this IExpression expression, IExpressionContext context)
        {
            return
                expression
                    .ValidateInItems(context)
                    .Invoke(context)
                    .ValidateOutItems(context);
        }

        public static IEnumerable<IExpression> InvokeWithValidation(this IEnumerable<IExpression> expressions, IExpressionContext context)
        {
            return
                from expression in expressions
                select
                    expression
                        .ValidateInItems(context)
                        .Invoke(context)
                        .ValidateOutItems(context);
        }

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

        //public static TExpression Log<TExpression>(this TExpression expression) where TExpression : IExpression
        //{
        //    return (TExpression)(ExpressionContextScope.Current.Result = expression);
        //}
    }
}