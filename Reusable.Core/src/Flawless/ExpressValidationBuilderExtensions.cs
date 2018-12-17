using System;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Flawless
{
    using static ExpressionFactory;

    [PublicAPI]
    public static class ExpressValidationBuilderExtensions
    {
        public static ExpressValidationRuleBuilder<T> True<T>(this ExpressValidationBuilder<T> builder, Expression<Func<T, bool>> expression)
        {
            return builder.Rule(expression);
        }

        public static ExpressValidationRuleBuilder<T> Null<T, TMember>(this ExpressValidationBuilder<T> builder, Expression<Func<T, TMember>> expression)
        {
            return
                builder
                    .True(CreateEqualNullExpression(expression))
                    .WithMessage($"{typeof(TMember).ToPrettyString()} must be null.")
                    .BreakOnFailure();
        }

        public static ExpressValidationRuleBuilder<T> Null<T>(this ExpressValidationBuilder<T> builder)
        {
            return
                builder
                    .True(CreateEqualNullExpression<T>())
                    .WithMessage($"{typeof(T).ToPrettyString()} must be null.")
                    .BreakOnFailure();
        }

        public static ExpressValidationRuleBuilder<T> False<T>(this ExpressValidationBuilder<T> builder, Expression<Func<T, bool>> expression)
        {
            return builder.True(Negate(expression));
        }

        public static ExpressValidationRuleBuilder<T> NotNull<T, TMember>(this ExpressValidationBuilder<T> builder, Expression<Func<T, TMember>> expression)
        {
            return
                builder
                    .False(CreateEqualNullExpression(expression))
                    .WithMessage($"{typeof(TMember).ToPrettyString()} must not be null.")
                    .BreakOnFailure();
        }

        public static ExpressValidationRuleBuilder<T> NotNull<T>(this ExpressValidationBuilder<T> builder)
        {
            return
                builder
                    .False(CreateEqualNullExpression<T>())
                    .WithMessage($"{typeof(T).ToPrettyString()} must not be null.")
                    .BreakOnFailure();
        }
    }

    internal static class ExpressionFactory
    {
        public static Expression<Func<T, bool>> CreateEqualNullExpression<T>()
        {
            return CreateEqualNullExpression<T>(Expression.Parameter(typeof(T)));
        }

        public static Expression<Func<T, bool>> CreateEqualNullExpression<T>(ParameterExpression parameter)
        {
            var equalNullExpression =
                Expression.Lambda<Func<T, bool>>(
                    Expression.ReferenceEqual(
                        parameter,
                        Expression.Constant(default(T))),
                    parameter
                );

            return equalNullExpression;
        }

        public static Expression<Func<T, bool>> CreateEqualNullExpression<T, TMember>(Expression<Func<T, TMember>> memberExpression)
        {
            // T.Member == null
            var equalNullExpression =
                Expression.Lambda<Func<T, bool>>(
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