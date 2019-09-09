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
        public static ValidationRuleBuilder<TValue> Required<TValue>(this ValidationRuleBuilder<TValue> builder) where TValue : class
        {
            return builder.Not().Null().Error();
        }
        
        public static ValidationRuleBuilder<string> NullOrEmpty(this string value)
        {
            return default; // expression => ValidationRule<T, TContext>.Builder.Predicate(_ => vef.ReferenceEqualNull(expression));
        }
        
        // --------------

        public static ValidationRuleBuilder<T> When<T>(this ValidationRuleBuilder<T> builder, Expression<Func<T, bool>> expression)
        {
            var validate = expression.AddContextParameterIfNotExists<T, bool>();
            var injected = ObjectInjector.Inject(validate, builder.ValueExpression.Body);
            var lambda = Expression.Lambda(injected, builder.ValueExpression.Parameters);
            return builder.When(lambda);
        }

        public static ValidationRuleBuilder<T> Null<T>(this ValidationRuleBuilder<T> builder)
        {
            return builder.Predicate(exprfac.ReferenceEqualNull);
        }

        public static IValidationRuleBuilder<T> NullOrEmpty<T>(this IValidationRuleBuilder<T> builder)
        {
            return builder.Predicate(exprfac.IsNullOrEmpty);
        }

        public static ValidationRuleBuilder<T> GreaterThan<T>(this ValidationRuleBuilder<T> builder, T value)
        {
            return builder.Predicate(expression => exprfac.GreaterThan(expression, value));
        }

        public static ValidationRuleBuilder<T> Like<T>(this ValidationRuleBuilder<T> builder, [RegexPattern] string pattern, RegexOptions options = RegexOptions.None)
        {
            return builder.Predicate(expression => exprfac.IsMatch(expression, pattern, options));
        }

        public static ValidationRuleBuilder<T> Equal<T>(this ValidationRuleBuilder<T> builder, T value, IEqualityComparer<T> comparer = default)
        {
            return builder.Predicate(expression => exprfac.Equal(expression, value, comparer ?? EqualityComparer<T>.Default));
        }

        public static ValidationRuleBuilder<T> Message<T>(this ValidationRuleBuilder<T> builder, string message)
        {
            return builder.Message(Expression.Lambda(Expression.Constant(message), builder.ValueExpression.Parameters));
        }
    }
}