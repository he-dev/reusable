using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Reusable.Flawless.ExpressionVisitors;

namespace Reusable.Flawless.Helpers
{
    internal static class ValidationExpressionFactory
    {
        public static LambdaExpression ReferenceEqualNull<T>()
        {
            return ReferenceEqualNull<T>(Expression.Parameter(typeof(T)));
        }

        public static LambdaExpression ReferenceEqualNull<T>(Expression<Func<T>> expression)
        {
            // x => object.ReferenceEqual(x.Member, null)

            // This is tricky because the original expression is () => (<>c__DisplayClass).x.y.z
            // We first need to the closure and inject out parameter there.
            var member = ValidationClosureSearch.FindParameter(expression);
            var parameter = Expression.Parameter(member.Type);
            var expressionWithParameter = ValidationParameterInjector.InjectParameter(expression.Body, parameter);
            return ReferenceEqualNull<T>(parameter, expressionWithParameter);
        }

        public static LambdaExpression ReferenceEqualNull<T, TMember>(Expression<Func<T, TMember>> expression)
        {
            // x => object.ReferenceEqual(x.Member, null)

            // This is tricky because the original expression is () => (<>c__DisplayClass).x.y.z
            // We first need to the closure and inject out parameter there.
            //var member = ValidationClosureSearch.FindParameter(expression);
            //var parameter = Expression.Parameter(member.Type);
            //var expressionWithParameter = ValidationParameterInjector.InjectParameter(expression.Body, parameter);

            // x => object.ReferenceEqual(x, null)
            return
                Expression.Lambda(
                    Expression.ReferenceEqual(
                        expression.Body,
                        Expression.Constant(default(T))),
                    expression.Parameters
                );
        }

        private static LambdaExpression ReferenceEqualNull<T>(ParameterExpression parameter, Expression value = default)
        {
            // x => object.ReferenceEqual(x, null)
            return
                Expression.Lambda(
                    Expression.ReferenceEqual(
                        value ?? parameter,
                        Expression.Constant(default(T))),
                    parameter
                );
        }

        private static LambdaExpression ReferenceEqualNull<T, TMember>(ParameterExpression parameter, Expression value = default)
        {
            // x => object.ReferenceEqual(x, null)
            return
                Expression.Lambda(
                    Expression.ReferenceEqual(
                        value ?? parameter,
                        Expression.Constant(default(T))),
                    parameter
                );
        }

        public static LambdaExpression NullOrEmpty<T>(Expression<Func<T, string>> expression)
        {
            // x => string.IsNullOrEmpty(x)
            var isNullOrEmptyMethod = typeof(string).GetMethod(nameof(string.IsNullOrEmpty));
            return
                Expression.Lambda(
                    Expression.Call(
                        isNullOrEmptyMethod,
                        expression.Body),
                    expression.Parameters
                );
        }

        public static LambdaExpression Match<T>(Expression<Func<T, string>> expression, string pattern, RegexOptions options)
        {
            // x => Regex.IsMatch(x, pattern, options)
            var isMatchMethod = typeof(Regex).GetMethod(nameof(Regex.IsMatch), new [] { typeof(string), typeof(string), typeof(RegexOptions) });
            return
                Expression.Lambda(
                    Expression.Call(
                        isMatchMethod,
                        expression.Body,
                        Expression.Constant(pattern),
                        Expression.Constant(options)),
                    expression.Parameters
                );
        }

        public static LambdaExpression Not(LambdaExpression expression)
        {
            // !x
            return
                Expression.Lambda(
                    Expression.Not(expression.Body),
                    expression.Parameters
                );
        }
    }
}