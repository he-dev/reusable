using System;
using System.Linq.Expressions;
using Reusable.Extensions;

namespace Reusable.Flawless
{
    using static ValidationExpressionFactory;

    public static class ValidationRuleBuilderExtension
    {
        public static ValidationRuleBuilder True(this ValidationRuleBuilder builder, Expression<Func<bool>> expression)
        {
            return
                builder
                    .Predicate(expression)
                    .Message(() => "The specified expression must be 'true'.");
        }

        public static ValidationRuleBuilder Null<TMember>(this ValidationRuleBuilder builder, Expression<Func<TMember>> expression)
        {
            return
                builder
                    .Predicate(ReferenceEqualNull(expression))
                    .Message(() => $"{typeof(TMember).ToPrettyString(false)} must be null.");
        }

        public static ValidationRuleBuilder Null<T>(this ValidationRuleBuilder builder, T value)
        {
            return
                builder
                    .Predicate(ReferenceEqualNull<T>())
                    .Message(() => $"{typeof(T).ToPrettyString(false)} must be null.");
        }

        public static ValidationRuleBuilder False<T>(this ValidationRuleBuilder builder, Expression<Func<bool>> expression)
        {
            return
                builder
                    .Predicate(Negate<T>(expression))
                    .Message(() => "The specified expression must be 'false'.");
        }

        public static ValidationRuleBuilder NotNull<TMember>(this ValidationRuleBuilder builder, Expression<Func<TMember>> expression)
        {
            return
                builder
                    .Predicate(Negate<TMember>(ReferenceEqualNull(expression)))
                    .Message(() => $"{typeof(TMember).ToPrettyString(false)} must not be null.");
        }

        public static ValidationRuleBuilder NotNull<T>(this ValidationRuleBuilder builder, T value)
        {
            return
                builder
                    .Predicate(Negate<T>(ReferenceEqualNull<T>()))
                    .Message(() => $"{typeof(T).ToPrettyString(false)} must not be null.");
        }
    }
}