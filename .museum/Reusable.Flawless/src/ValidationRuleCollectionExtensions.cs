using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Exceptionize;

namespace Reusable.Flawless
{
    public delegate ValidationRuleBuilder<T, TContext> BuildRuleCallback<T, TContext>(ValidationRuleBuilder<T, TContext> builder);

    [PublicAPI]
    public static class ValidationRuleCollectionExtensions
    {
        #region Rule APIs

        public static ValidationRuleCollection<T, TContext> Accept<T, TContext>(this ValidationRuleCollection<T, TContext> rules, BuildRuleCallback<T, TContext> builder)
        {
            return rules.Add(builder(ValidationRuleBuilder<T, TContext>.Empty).Build());
        }

        public static ValidationRuleCollection<T, object> Accept<T>(this ValidationRuleCollection<T, object> rules, BuildRuleCallback<T, object> builder)
        {
            return rules.Add(builder(ValidationRuleBuilder<T, object>.Empty).Build());
        }

        public static ValidationRuleCollection<T, TContext> Reject<T, TContext>(this ValidationRuleCollection<T, TContext> rules, BuildRuleCallback<T, TContext> builder)
        {
            return rules.Add(builder(ValidationRuleBuilder<T, TContext>.Empty).Negate().Build());
        }

        public static ValidationRuleCollection<T, object> Reject<T>(this ValidationRuleCollection<T, object> rules, BuildRuleCallback<T, object> builder)
        {
            return rules.Add(builder(ValidationRuleBuilder<T, object>.Empty).Negate().Build());
        }

        #endregion

        [NotNull]
        public static ValidationResultCollection<T> ValidateWith<T, TContext>(this T obj, ValidationRuleCollection<T, TContext> rules, TContext context)
        {
            try
            {
                return new ValidationResultCollection<T>(obj, rules.Evaluate(obj, context).ToImmutableList());
            }
            catch (Exception inner)
            {
                throw DynamicException.Create
                (
                    $"UnexpectedValidation",
                    $"An unexpected error occured. See the inner exception for details.",
                    inner
                );
            }
        }

        [NotNull]
        public static ValidationResultCollection<T> ValidateWith<T, TContext>(this T obj, ValidationRuleCollection<T, TContext> rules)
        {
            return obj.ValidateWith(rules, default);
        }

        private static IEnumerable<IValidationResult> Evaluate<T, TContext>(this ValidationRuleCollection<T, TContext> rules, T obj, TContext context)
        {
            foreach (var result in rules.Select(r => r.Evaluate(obj, context)))
            {
                yield return result;

                if (result is ValidationError)
                {
                    yield break;
                }
            }
        }
    }
}