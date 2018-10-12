using System;
using System.Collections.Generic;
using System.Linq.Custom;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Validation
{
    [PublicAPI]
    public static class WeelidatorExtensions2
    {
        #region Ensure overloads

        public static WeelidatorRuleBuilder<T> Ensure<T>(this WeelidatorBuilder<T> builder, Expression<Func<T, bool>> expression)
        {
            return builder.NewRule(expression);
        }

        public static WeelidatorRuleBuilder<T> EnsureNull<T>(this WeelidatorBuilder<T> builder)
        {
            return
                builder
                    .Ensure(IsNullExpression.Create<T>())
                    .WithMessage($"{typeof(T).ToPrettyString()} must be null.")
                    .BreakOnFailure();
        }

        #endregion

        #region Block overloads

        public static WeelidatorRuleBuilder<T> Block<T>(this WeelidatorBuilder<T> builder, Expression<Func<T, bool>> expression)
        {
            var notExpression = Expression.Lambda<Func<T, bool>>(Expression.Not(expression.Body), expression.Parameters[0]);
            return builder.Ensure(notExpression);
        }

        public static WeelidatorRuleBuilder<T> BlockNull<T>(this WeelidatorBuilder<T> builder)
        {
            return
                builder
                    .Block(IsNullExpression.Create<T>())
                    .WithMessage($"{typeof(T).ToPrettyString()} must not be null.")
                    .BreakOnFailure();
        }

        #endregion
    }

    internal static class IsNullExpression
    {
        public static Expression<Func<T, bool>> Create<T>(Expression<Func<T, bool>> expression)
        {
            return Create<T>(expression.Parameters[0]);
        }

        public static Expression<Func<T, bool>> Create<T>()
        {
            return Create<T>(Expression.Parameter(typeof(T)));
        }

        private static Expression<Func<T, bool>> Create<T>(ParameterExpression parameter)
        {
            //var parameter = Expression.Parameter(typeof(T));
            var equalsNullExpression =
                Expression.Lambda<Func<T, bool>>(
                    Expression.ReferenceEqual(
                        parameter,
                        Expression.Constant(default(T))),
                    parameter
                );

            return equalsNullExpression;
        }
    }
}