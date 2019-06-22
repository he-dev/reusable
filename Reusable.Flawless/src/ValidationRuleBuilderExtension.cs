using System;
using System.Linq.Expressions;
using Reusable.Extensions;

namespace Reusable.Flawless
{
    using static ValidationExpressionFactory;

    public static class ValidationRuleBuilderExtension
    {
        public static ValidationRuleBuilder<T> True<T>(this ValidationRuleBuilder<T> builder, Expression<Func<bool>> expression)
        {
            return builder.Predicate(InjectParameter<T>(expression));
        }

        public static ValidationRuleBuilder<T> Null<T, TMember>(this ValidationRuleBuilder<T> builder, Expression<Func<TMember>> expression)
        {
            return
                builder
                    .True(ReferenceEqualNull(expression))
                    .Message(() => $"{typeof(TMember).ToPrettyString(false)} must be null.");
        }

        public static ValidationRuleBuilder<T> Null<T>(this ValidationRuleBuilder<T> builder)
        {
            return
                builder
                    .Predicate(ReferenceEqualNull<T>())
                    .Message(() => $"{typeof(T).ToPrettyString(false)} must be null.");
        }

        public static ValidationRuleBuilder<T> False<T>(this ValidationRuleBuilder<T> builder, Expression<Func<bool>> expression)
        {
            return builder.Predicate(Negate(InjectParameter<T>(expression)));
        }

        public static ValidationRuleBuilder<T> NotNull<T, TMember>(this ValidationRuleBuilder<T> builder, Expression<Func<TMember>> expression)
        {
            return
                builder
                    .False(ReferenceEqualNull(expression))
                    .Message(() => $"{typeof(TMember).ToPrettyString(false)} must not be null.");
        }

        public static ValidationRuleBuilder<T> NotNull<T>(this ValidationRuleBuilder<T> builder)
        {
            return
                builder
                    .Predicate(Negate(ReferenceEqualNull<T>()))
                    .Message(() => $"{typeof(T).ToPrettyString(false)} must not be null.");
        }

        private static Expression<Func<T, bool>> InjectParameter<T>(Expression<Func<bool>> expression)
        {
            var parameter = ValidationParameterPrettifier.CreatePrettyParameter<T>();
            var expressionWithParameter = ValidationParameterInjector.InjectParameter(expression, parameter);
            return Expression.Lambda<Func<T, bool>>(expressionWithParameter, parameter);
        }
    }
}