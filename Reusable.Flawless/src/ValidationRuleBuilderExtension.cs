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
    using exprfac = ValidationExpressionFactory;

    public static class ValidationRuleBuilderExtension
    {
        public static IValidationRuleBuilder<T, TValue> When<T, TValue>(this IValidationRuleBuilder<T, TValue> builder, Expression<Func<T, bool>> when)
        {
            return builder.When(when.AddContextParameter());
        }

        public static IValidationRuleBuilder<T, TValue> Null<T, TValue>(this IValidationRuleBuilder<T, TValue> builder)
        {
            return builder.Predicate(x => ReferenceEquals(x, null));
        }

        public static IValidationRuleBuilder<T, string> NullOrEmpty<T>(this IValidationRuleBuilder<T, string> builder)
        {
            return builder.Predicate(x => string.IsNullOrEmpty(x));
        }

        public static IValidationRuleBuilder<T, TValue> GreaterThan<T, TValue>(this IValidationRuleBuilder<T, TValue> builder, TValue value)
        {
            return builder.Predicate(x => exprfac.GreaterThan(() => x, value).Compile()(x));
        }

        public static IValidationRuleBuilder<T, string> Like<T>(this IValidationRuleBuilder<T, string> builder, [RegexPattern] string pattern, RegexOptions options = RegexOptions.None)
        {
            return builder.Predicate(x => Regex.IsMatch(x, pattern, options));
        }

        public static IValidationRuleBuilder<T, TValue> Equal<T, TValue>(this IValidationRuleBuilder<T, TValue> builder, TValue value, IEqualityComparer<TValue> comparer = default)
        {
            comparer = comparer ?? EqualityComparer<TValue>.Default;
            return builder.Predicate(x => comparer.Equals(x, value));
        }

        public static IValidationRuleBuilder<T, TValue> Message<T, TValue>(this IValidationRuleBuilder<T, TValue> builder, string message)
        {
            //return builder.Message( Expression.Lambda(Expression.Constant(message), builder.ValueSelector.Parameters));
            return builder.Message((x, context) => message);
        }

        public static IValidationRuleBuilder<T, TValue> Required<T, TValue>(this IValidationRuleBuilder<T, TValue> builder) where TValue : class
        {
            return builder.Not().Null().Error();
        }
    }
}