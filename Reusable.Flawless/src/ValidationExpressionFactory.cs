using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Flawless
{
    using static ValidationExpressionFactory;

    internal static class ValidationExpressionFactory
    {
        public static Expression<Func<T, bool>> ReferenceEqualNull<T>()
        {
            return ReferenceEqualNull<T>(Expression.Parameter(typeof(T)));
        }

        public static Expression<Func<T, bool>> ReferenceEqualNull<T>(ParameterExpression parameter)
        {
            // x => object.ReferenceEqual(x, null)
            var equalNullExpression =
                Expression.Lambda<Func<T, bool>>(
                    Expression.ReferenceEqual(
                        parameter,
                        Expression.Constant(default(T))),
                    parameter
                );

            return equalNullExpression;
        }

        public static Expression<Func<bool>> ReferenceEqualNull<TMember>(Expression<Func<TMember>> memberExpression)
        {
            // T.Member == null
            var equalNullExpression =
                Expression.Lambda<Func<bool>>(
                    Expression.ReferenceEqual(
                        memberExpression.Body,
                        Expression.Constant(default(TMember))
                    ),
                    memberExpression.Parameters[0]
                );

            return equalNullExpression;
        }

        public static Expression<Func<T, bool>> Negate<T>(Expression<Func<T, bool>> expression)
        {
            return
                Expression.Lambda<Func<T, bool>>
                (
                    Expression.Not(expression.Body),
                    expression.Parameters[0]
                );
        }
    }
}