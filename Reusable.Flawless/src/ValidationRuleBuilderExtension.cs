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

    public abstract class When
    {
        public static When Value = default;
    }
    
    public static class ValidationRuleBuilderExtension
    {
        
        public static ValidationRuleBuilder<TValue> Required<TValue>(this ValidationRuleBuilder<TValue> builder) where TValue : class
        {
            return builder.Not().Null().Error();
        }
        
//        public static ValidationRuleBuilder<TValue, object> Not<TValue>(this ValidationRuleBuilder<TValue, object> builder)
//        {
//            return default;// expression => ValidationRule<T, TContext>.Builder.Predicate(_ => vef.ReferenceEqualNull(expression));
//        }
        
        public static ValidationRuleBuilder<string> NullOrEmpty(this string value)
        {
            return default;// expression => ValidationRule<T, TContext>.Builder.Predicate(_ => vef.ReferenceEqualNull(expression));
        }
        
        
        // --------------
        
        public static ValidationRuleBuilder<T> When<T>(this ValidationRuleBuilder<T> builder, Expression<Func<T, bool>> expression)
        {
            return builder.Predicate(_ => expression);
        }

        public static ValidationRuleBuilder<T> Null<T>(this ValidationRuleBuilder<T> builder)
        {
            return builder.Predicate(exprfac.ReferenceEqualNull);
        }
        

        public static ValidationRuleBuilder<T> NullOrEmpty<T, TContext>(this ValidationRuleBuilder<T> builder, Expression<Func<T, string>> expression)
        {
            return builder.Predicate(_ => exprfac.IsNullOrEmpty(expression));
        }

        public static ValidationRuleBuilder<T> Like<T>
        (
            this ValidationRuleBuilder<T> builder,
            Expression<Func<T, string>> expression,
            [RegexPattern] string pattern,
            RegexOptions options = RegexOptions.None
        )
        {
            return builder.Predicate(_ => exprfac.IsMatch(expression, pattern, options));
        }

        public static ValidationRuleBuilder<T> Equal<T>
        (
            this ValidationRuleBuilder<T> builder,
            //Expression<Func<T, TMember>> expression,
            T value,
            IEqualityComparer<T> comparer = default
        )
        {
            return builder.Predicate(expression => exprfac.Equal(expression, value, comparer ?? EqualityComparer<T>.Default));
        }

        internal static ValidationRuleBuilder<T> Negate<T>(this ValidationRuleBuilder<T> builder)
        {
            return builder.Predicate(exprfac.Not);
        }

        public static ValidationRuleBuilder<T> Message<T>(this ValidationRuleBuilder<T> builder, string message)
        {
            return builder.Message(x => message);
        }
    }
}