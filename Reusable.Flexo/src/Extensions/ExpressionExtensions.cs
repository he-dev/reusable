using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Reusable.Flexo.Annotations;
using Reusable.Flexo.Diagnostics;
using Reusable.Flexo.Expressions;

namespace Reusable.Flexo.Extensions
{
    public static class ExpressionExtensions
    {
        public static TExpression ValidateInParameters<TExpression>(this TExpression expression, IExpressionContext context) where TExpression : IExpression
        {
            var missingInParameters =
                typeof(TExpression)
                    .GetCustomAttributes<InParameterAttribute>()
                    .Where(parameter => parameter.Required && !context.Parameters.ContainsKey(parameter.Name))
                    .ToList();

            return
                missingInParameters.Any()
                    ? throw new MissingInParameterException(missingInParameters)
                    : expression;
        }

        public static IEnumerable<TExpression> ValidateInParameters<TExpression>(this IEnumerable<TExpression> expressions, IExpressionContext context) where TExpression : IExpression
        {
            return expressions.Select(expression => expression.ValidateParameters<TExpression, InParameterAttribute>(context));
        }

        public static TExpression ValidateOutParameters<TExpression>(this TExpression expression, IExpressionContext context) where TExpression : IExpression
        {
            return expression.ValidateParameters<TExpression, OutParameterAttribute>(context);
        }

        public static IEnumerable<TExpression> ValidateOutParameters<TExpression>(this IEnumerable<TExpression> expressions, IExpressionContext context) where TExpression : IExpression
        {
            return expressions.Select(expression => expression.ValidateParameters<TExpression, OutParameterAttribute>(context));
        }

        public static TExpression ValidateParameters<TExpression, TParameterAttribute>(this TExpression expression, IExpressionContext context) where TExpression : IExpression where TParameterAttribute : Attribute, IParameterAttribute
        {
            var missingParameters =
                typeof(TExpression)
                    .GetCustomAttributes<TParameterAttribute>()
                    .Where(parameter => parameter.Required && !context.Parameters.ContainsKey(parameter.Name))
                    .ToList();

            if (missingParameters.Any())
            {
                if (typeof(TParameterAttribute) == typeof(OutParameterAttribute)) throw new MissingOutParameterException(missingParameters);
                if (typeof(TParameterAttribute) == typeof(InParameterAttribute)) throw new MissingInParameterException(missingParameters);
                throw new ArgumentException($"Inalid parameter attribute: {typeof(TParameterAttribute).FullName}");
            }
            return expression;
        }

        ///// <summary>
        ///// Gets only enabled expressions.
        ///// </summary>
        //public static IEnumerable<T> Enabled<T>(this IEnumerable<T> expressions) where T : ISwitchable
        //{
        //    return expressions.Where(x => x.Enabled);
        //}

        public static IExpression InvokeWithValidation(this IExpression expression, IExpressionContext context)
        {
            return
                expression.Enabled
                    ? expression
                        .ValidateInParameters(context)
                        .Invoke(context)
                        .ValidateOutParameters(context)
                    : Expression.Empty;
        }

        public static IEnumerable<IExpression> InvokeWithValidation(this IEnumerable<IExpression> expressions, IExpressionContext context)
        {
            return
                from expression in expressions
                where expression.Enabled
                select
                    expression
                        .ValidateInParameters(context)
                        .Invoke(context)
                        .ValidateOutParameters(context);
        }

        public static IEnumerable<T> Values<T>(this IEnumerable<IExpression> expressions)
        {
            return
                from expression in expressions
                select expression.Value<T>();
        }

        /// <summary>
        /// Gets the value of a Constant expression and tries to cast it to T.
        /// </summary>
        public static T Value<T>(this IExpression expression)
        {
            return
                expression is Constant<T> constant
                    ? constant.Value
                    : throw new InvalidExpressionException(typeof(Constant<T>), expression.GetType());
        }

        public static T ValueOrDefault<T>(this IExpression expression)
        {
            return
                expression is Constant<T> constant
                    ? constant.Value
                    : default;
        }

        public static TExpression Log<TExpression>(this TExpression expression) where TExpression : IExpression
        {
            return (TExpression)(ExpressionContextScope.Current.Result = expression);
        }
    }
}