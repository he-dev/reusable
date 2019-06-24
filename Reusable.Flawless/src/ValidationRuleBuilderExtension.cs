using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Flawless.ExpressionVisitors;
using Reusable.Flawless.Helpers;

namespace Reusable.Flawless
{
    using vef = ValidationExpressionFactory;

    public static class ValidationRuleBuilderExtension
    {
        public static ValidationRuleBuilder<T, TContext> When<T, TContext>(this ValidationRuleBuilder<T, TContext> builder, Expression<Func<T, bool>> expression)
        {
            return
                builder
                    .Predicate(_ => expression);
        }

        public static ValidationRuleBuilder<T, TContext> Null<T, TContext, TMember>(this ValidationRuleBuilder<T, TContext> builder, Expression<Func<T, TMember>> expression)
        {
            return
                builder
                    .Predicate(_ => vef.ReferenceEqualNull(expression));
        }

        public static ValidationRuleBuilder<T, TContext> NullOrEmpty<T, TContext>(this ValidationRuleBuilder<T, TContext> builder, Expression<Func<T, string>> expression)
        {
            return
                builder
                    .Predicate(_ => vef.IsNullOrEmpty(expression));
        }

        public static ValidationRuleBuilder<T, TContext> Like<T, TContext>
        (
            this ValidationRuleBuilder<T, TContext> builder,
            Expression<Func<T, string>> expression,
            [RegexPattern] string pattern,
            RegexOptions options = RegexOptions.None
        )
        {
            return
                builder
                    .Predicate(_ => vef.IsMatch(expression, pattern, options));
        }

        public static ValidationRuleBuilder<T, TContext> Equal<T, TContext, TMember>
        (
            this ValidationRuleBuilder<T, TContext> builder,
            Expression<Func<T, TMember>> expression,
            TMember value,
            IEqualityComparer<TMember> comparer = default
        )
        {
            return
                builder
                    .Predicate(_ => vef.Equal(expression, value, comparer ?? EqualityComparer<TMember>.Default));
        }

        internal static ValidationRuleBuilder<T, TContext> Negate<T, TContext>(this ValidationRuleBuilder<T, TContext> builder)
        {
            return
                builder
                    .Predicate(vef.Not);
        }

        public static ValidationRuleBuilder<T, TContext> Message<T, TContext>(this ValidationRuleBuilder<T, TContext> builder, string message)
        {
            return
                builder
                    .Message((x, c) => message);
        }

        public static ValidationRuleBuilder<T, TContext> Hard<T, TContext>(this ValidationRuleBuilder<T, TContext> builder)
        {
            return
                builder
                    .Rule(ValidationRule<T, TContext>.Hard);
        }
    }
}