using System;
using System.Linq.Expressions;
using Reusable.Extensions;
using Reusable.Flawless.ExpressionVisitors;
using Reusable.Flawless.Helpers;

namespace Reusable.Flawless
{
    using static ValidationExpressionFactory;

    public static class ValidationRuleBuilderExtension
    {
        public static ValidationRuleBuilder<T, TContext> True<T, TContext>(this ValidationRuleBuilder<T, TContext> builder, Expression<Func<T, bool>> expression)
        {
            return
                builder
                    .Predicate(expression)
                    .Message("The specified expression must be 'true'.");
        }
        
        public static ValidationRuleBuilder<T, TContext> False<T, TContext>(this ValidationRuleBuilder<T, TContext> builder, Expression<Func<bool>> expression)
        {
            return
                builder
                    .Predicate(Not(expression))
                    .Message("The specified expression must be 'false'.");
        }

        public static ValidationRuleBuilder<T, TContext> Null<T, TContext, TMember>(this ValidationRuleBuilder<T, TContext> builder, Expression<Func<TMember>> expression)
        {
            return
                builder
                    .Predicate(ReferenceEqualNull(expression))
                    .Message($"{typeof(TMember).ToPrettyString(false)} must be null.");
        }

        public static ValidationRuleBuilder<T, TContext> Null<T, TContext>(this ValidationRuleBuilder<T, TContext> builder, T value)
        {
            return
                builder
                    .Predicate(ReferenceEqualNull<T>())
                    .Message($"{typeof(T).ToPrettyString(false)} must be null.");
        }

        

        public static ValidationRuleBuilder<T, TContext> NotNull<T, TContext, TMember>(this ValidationRuleBuilder<T, TContext> builder, Expression<Func<T, TMember>> expression)
        {
            return
                builder
                    .Predicate(Not(ReferenceEqualNull(expression)))
                    .Message($"{typeof(TMember).ToPrettyString(false)} must not be null.");
        }

        public static ValidationRuleBuilder<T, TContext> NotNull<T, TContext>(this ValidationRuleBuilder<T, TContext> builder, T value)
        {
            return
                builder
                    .Predicate(Not(ReferenceEqualNull<T>()))
                    .Message($"{typeof(T).ToPrettyString(false)} must not be null.");
        }
        
        public static ValidationRuleBuilder<T, TContext> NotNullOrEmpty<T, TContext>(this ValidationRuleBuilder<T, TContext> builder, Expression<Func<T, string>> expression)
        {
            return
                builder
                    .Predicate(Not(NullOrEmpty(expression)))
                    .Message($"'{ValidationParameterPrettifier.Prettify<T>(expression)}' must not be null or empty.");
        }

        public static ValidationRuleBuilder<T, TContext> Message<T, TContext>(this ValidationRuleBuilder<T, TContext> builder, string message)
        {
            return
                builder
                    .Message((x, c) => message);
        }
    }
}