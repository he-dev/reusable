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
        public static ValidationRuleBuilder<T, TValue> Required<T, TValue>(this ValidationRuleBuilder<T, TValue> builder) where TValue : class
        {
            return builder.Not().Null().Error();
        }
        
//        public static ValidationRuleBuilder<string> NullOrEmpty(this string value)
//        {
//            return default; // expression => ValidationRule<T, TContext>.Builder.Predicate(_ => vef.ReferenceEqualNull(expression));
//        }
        
        // --------------

        public static ValidationRuleBuilder<T, TValue> When<T, TValue>(this ValidationRuleBuilder<T, TValue> builder, Expression<Func<TValue, bool>> when)
        {
//            var validate = when.AddContextParameterIfNotExists<T, bool>();
//            var injected = ObjectInjector.Inject(validate, builder.ValueSelector.Body);
//            var lambda = Expression.Lambda(injected, builder.ValueSelector.Parameters);
            return builder.When(when);
        }

        public static ValidationRuleBuilder<T, TValue> Null<T, TValue>(this ValidationRuleBuilder<T, TValue> builder)
        {
            return builder.Predicate(exprfac.ReferenceEqualNull);
        }

        public static IValidationRuleBuilder<T> NullOrEmpty<T>(this IValidationRuleBuilder<T> builder)
        {
            return default;// builder.Predicate(exprfac.IsNullOrEmpty);
        }

        public static ValidationRuleBuilder<T, TValue> GreaterThan<T, TValue>(this ValidationRuleBuilder<T, TValue> builder, TValue value)
        {
            return default;// builder.Predicate(expression => exprfac.GreaterThan(expression, value));
        }

        public static ValidationRuleBuilder<T, TValue> Like<T, TValue>(this ValidationRuleBuilder<T, TValue> builder, [RegexPattern] string pattern, RegexOptions options = RegexOptions.None)
        {
            return default;// builder.Predicate(expression => exprfac.IsMatch(expression, pattern, options));
        }

        public static ValidationRuleBuilder<T, TValue> Equal<T, TValue>(this ValidationRuleBuilder<T, TValue> builder, TValue value, IEqualityComparer<TValue> comparer = default)
        {
            return default;// builder.Predicate(expression => exprfac.Equal(expression, value, comparer ?? EqualityComparer<T>.Default));
        }

        public static ValidationRuleBuilder<T, TValue> Message<T, TValue>(this ValidationRuleBuilder<T, TValue> builder, string message)
        {
            return default;// builder.Message(Expression.Lambda(Expression.Constant(message), builder.ValueSelector.Parameters));
        }
    }
}