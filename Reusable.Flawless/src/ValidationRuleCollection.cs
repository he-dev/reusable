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
        public static IImmutableList<IValidationRule<T, TContext>> Add<T, TContext>(this IImmutableList<IValidationRule<T, TContext>> rules, Func<T, TContext, ValidationRuleBuilder<T>> builder)
        {
            return rules.Add(builder(default, default).Build<TContext>());
        }
        
        public static ILookup<bool, IValidationResult<T>> ValidateWith<T, TContext>(this T obj, IImmutableList<IValidationRule<T, TContext>> rules, TContext context)
        {
            return
                rules
                    .Evaluate(obj, context)
                    .ToLookup(r => r.Success);
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