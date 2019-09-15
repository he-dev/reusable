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
        public static IValidatorBuilder<T> When<T, TValue>(this IValidatorBuilder<T> builder, Expression<Func<T, bool>> when)
        {
            return builder.When(when.AddContextParameter());
        }

        public static IValidatorBuilder<T> Null<T>(this IValidatorBuilder<T> builder)
        {
            return builder.Predicate(x => ReferenceEquals(x, null));
        }

        public static IValidatorBuilder<string> NullOrEmpty<T>(this IValidatorBuilder<string> builder)
        {
            return builder.Predicate(x => string.IsNullOrEmpty(x));
        }

        public static IValidatorBuilder<T> GreaterThan<T>(this IValidatorBuilder<T> builder, T value, IComparer<T> comparer = default)
        {
            return builder.Predicate(x => x.GreaterThan(value, comparer));
        }
        
        public static IValidatorBuilder<T> GreaterThanOrEqual<T>(this IValidatorBuilder<T> builder, T value)
        {
            return builder.Predicate(x => exprfac.Binary(() => x, value, Expression.GreaterThanOrEqual).Compile()(x));
        }

        public static IValidatorBuilder<string> Like<T>(this IValidatorBuilder<string> builder, [RegexPattern] string pattern, RegexOptions options = RegexOptions.None)
        {
            return builder.Predicate(x => Regex.IsMatch(x, pattern, options));
        }

        public static IValidatorBuilder<T> Equal<T>(this IValidatorBuilder<T> builder, T value, IEqualityComparer<T> comparer = default)
        {
            comparer = comparer ?? EqualityComparer<T>.Default;
            return builder.Predicate(x => comparer.Equals(x, value));
        }

        public static IValidatorBuilder<T> Message<T, TValue>(this IValidatorBuilder<T> builder, string message)
        {
            //return builder.Message( Expression.Lambda(Expression.Constant(message), builder.ValueSelector.Parameters));
            return builder.Message((x, context) => message);
        }

        public static IValidatorBuilder<T> Required<T>(this IValidatorBuilder<T> builder) where T : class
        {
            return builder.Not().Null().Error();
        }
    }
}