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

    public static class ComparerHelper
    {
        public static bool GreaterThan<T>(this T left, T right, IComparer<T> comparer = default)
        {
            return comparer.OrDefault().Compare(left, right) > 0;
        }
        
        public static bool GreaterThanOrEqual<T>(this T left, T right, IComparer<T> comparer = default)
        {
            return comparer.OrDefault().Compare(left, right) >= 0;
        }
        
        public static bool Equal<T>(this T left, T right, IComparer<T> comparer = default)
        {
            return comparer.OrDefault().Compare(left, right) == 0;
        }
        
        public static bool LessThan<T>(this T left, T right, IComparer<T> comparer = default)
        {
            return comparer.OrDefault().Compare(left, right) < 0;
        }
        
        public static bool LessThanOrEqual<T>(this T left, T right, IComparer<T> comparer = default)
        {
            return comparer.OrDefault().Compare(left, right) <= 0;
        }

        private static IComparer<T> OrDefault<T>(this IComparer<T> comparer) => comparer ?? Comparer<T>.Default;
    }

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

        public static IValidationRuleBuilder<T, TValue> GreaterThan<T, TValue>(this IValidationRuleBuilder<T, TValue> builder, TValue value, IComparer<TValue> comparer = default)
        {
            return builder.Predicate(x => x.GreaterThan(value, comparer));
        }
        
        public static IValidationRuleBuilder<T, TValue> GreaterThanOrEqual<T, TValue>(this IValidationRuleBuilder<T, TValue> builder, TValue value)
        {
            return builder.Predicate(x => exprfac.Binary(() => x, value, Expression.GreaterThanOrEqual).Compile()(x));
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