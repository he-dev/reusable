using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Reusable.Flawless
{
    public static class ValidationRuleCollectionExtensions
    {
        public static IImmutableList<IValidationRule<T, TContext>> Add<T, TContext>
        (
            this IImmutableList<IValidationRule<T, TContext>> rules,
            Func<ValidationRuleBuilder<T, TContext>, ValidationRuleBuilder<T, TContext>> builder
        )
        {
            return rules.Add(builder(ValidationRuleBuilder<T, TContext>.Empty).Build());
        }

        public static IImmutableList<IValidationRule<T, object>> Add<T>
        (
            this IImmutableList<IValidationRule<T, object>> rules,
            Func<ValidationRuleBuilder<T, object>, ValidationRuleBuilder<T, object>> builder
        )
        {
            return rules.Add(builder(ValidationRuleBuilder<T, object>.Empty).Build());
        }

        #region Rule APIs
        
        public static IImmutableList<IValidationRule<T, object>> Accept<T>
        (
            this IImmutableList<IValidationRule<T, object>> rules,
            Func<ValidationRuleBuilder<T, object>, ValidationRuleBuilder<T, object>> builder
        )
        {
            return rules.Add(builder(ValidationRuleBuilder<T, object>.Empty).Build());
        }
        
        public static IImmutableList<IValidationRule<T, object>> Reject<T>
        (
            this IImmutableList<IValidationRule<T, object>> rules,
            Func<ValidationRuleBuilder<T, object>, ValidationRuleBuilder<T, object>> builder
        )
        {
            return rules.Add(builder(ValidationRuleBuilder<T, object>.Empty).Negate().Build());
        }

        #endregion

        public static ValidationResultCollection<T> ValidateWith<T, TContext>(this T obj, IImmutableList<IValidationRule<T, TContext>> rules, TContext context)
        {
            return new ValidationResultCollection<T>(obj, rules.Evaluate(obj, context).ToImmutableList());
        }

        public static ValidationResultCollection<T> ValidateWith<T, TContext>(this T obj, IImmutableList<IValidationRule<T, TContext>> rules)
        {
            return obj.ValidateWith(rules, default);
        }

        private static IEnumerable<IValidationResult> Evaluate<T, TContext>(this IImmutableList<IValidationRule<T, TContext>> rules, T obj, TContext context)
        {
            var result = default(IValidationResult);
            foreach (var rule in rules)
            {
                yield return result = rule.Evaluate(obj, context);
                if (result is Error) yield break;
            }
        }
    }
}