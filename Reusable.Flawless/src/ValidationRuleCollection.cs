using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Flawless
{
    public static class ValidationRuleCollection
    {
        public static IImmutableList<IValidationRule<T, TContext>> For<T, TContext>() => ImmutableList<IValidationRule<T, TContext>>.Empty;

        public static IImmutableList<IValidationRule<T, object>> For<T>() => ImmutableList<IValidationRule<T, object>>.Empty;
    }

    public static class ValidationRuleCollectionExtensions
    {
        public static IImmutableList<IValidationRule<T, TContext>> Add<T, TContext>(this IImmutableList<IValidationRule<T, TContext>> rules, Func<T, TContext, ValidationRuleBuilder> builder)
        {
            return rules.Add(builder(default, default).Build<T, TContext>());
        }
        
        public static IImmutableList<IValidationRule<T, object>> Add<T>(this IImmutableList<IValidationRule<T, object>> rules, Func<T, ValidationRuleBuilder> builder)
        {
            return rules.Add(builder(default).Build<T, object>());
        }
        
        public static IImmutableList<IValidationRule<T, object>> Require<T>(this IImmutableList<IValidationRule<T, object>> rules, Func<ValidationRuleBuilder, T, ValidationRuleBuilder> builder)
        {
            
            return rules.Add(builder(ValidationRule.Require, default).Build<T, object>());
        }
        
        public static IImmutableList<IValidationRule<T, object>> Ensure<T>(this IImmutableList<IValidationRule<T, object>> rules, Func<ValidationRuleBuilder, T, ValidationRuleBuilder> builder)
        {
            
            return rules.Add(builder(ValidationRule.Ensure, default).Build<T, object>());
        }

        public static (T Value, ILookup<bool, IValidationResult<T>> Results) ValidateWith<T, TContext>(this T obj, IImmutableList<IValidationRule<T, TContext>> rules, TContext context)
        {
            return
            (
                obj,
                rules
                    .Evaluate(obj, context)
                    .ToLookup(r => r.Success)
            );
        }

        public static (T Value, ILookup<bool, IValidationResult<T>> Results) ValidateWith<T>(this T obj, IImmutableList<IValidationRule<T, object>> rules)
        {
            return obj.ValidateWith(rules, default);
        }

        private static IEnumerable<IValidationResult<T>> Evaluate<T, TContext>(this IImmutableList<IValidationRule<T, TContext>> rules, T obj, TContext context)
        {
            var result = default(IValidationResult<T>);
            foreach (var rule in rules)
            {
                yield return result = rule.Evaluate(obj, context);
                if (!result.Success && rule.Option == ValidationRuleOption.Require) yield break;
            }
        }
    }
}