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
        public static IValidationRuleBuilder<T, TValue> Required<T, TValue>(this IValidationRuleBuilder<T, TValue> builder) where TValue : class
        {
            return builder.Not().Null().Error();
        }

        //        public static ValidationRuleBuilder<string> NullOrEmpty(this string value)
        //        {
        //            return default; // expression => ValidationRule<T, TContext>.Builder.Predicate(_ => vef.ReferenceEqualNull(expression));
        //        }

        // --------------

        public static IValidationRuleBuilder<T, TValue> When<T, TValue>(this IValidationRuleBuilder<T, TValue> builder, Expression<Func<T, bool>> when)
        {
            var validate = when.AddContextParameterIfNotExists<T, bool>();
            //var injected = ObjectInjector.Inject(validate, builder.Selector.Body);
            //var lambda = Expression.Lambda<ValidationFunc<T, bool>>(injected, when.Parameters);
            return builder.When(validate);
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

        public static IValidationRuleBuilder<T, TValue> Like<T, TValue>(this IValidationRuleBuilder<T, TValue> builder, [RegexPattern] string pattern, RegexOptions options = RegexOptions.None)
        {
            return default; // builder.Predicate(expression => exprfac.IsMatch(expression, pattern, options));
        }

        public static IValidationRuleBuilder<T, TValue> Equal<T, TValue>(this IValidationRuleBuilder<T, TValue> builder, TValue value, IEqualityComparer<TValue> comparer = default)
        {
            return default; // builder.Predicate(expression => exprfac.Equal(expression, value, comparer ?? EqualityComparer<T>.Default));
        }

        public static IValidationRuleBuilder<T, TValue> Message<T, TValue>(this IValidationRuleBuilder<T, TValue> builder, string message)
        {
            return default; // builder.Message(Expression.Lambda(Expression.Constant(message), builder.ValueSelector.Parameters));
        }
    }
}