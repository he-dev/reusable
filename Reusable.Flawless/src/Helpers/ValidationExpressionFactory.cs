using System;
using System.Linq.Expressions;
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

        public static LambdaExpression Negate(LambdaExpression expression)
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