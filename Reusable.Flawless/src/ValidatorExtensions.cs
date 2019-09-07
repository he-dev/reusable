using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable.Flawless
{
    public delegate ValidationRuleBuilder<T> BuildRuleCallback<T, TContext>(ValidationRuleBuilder<T> builder);

    public delegate ValidationRuleBuilder<T> BuilderCallback<T, TContext, TValue>(Expression<Func<T, TValue>> expression);

    [PublicAPI]
    public static class ValidatorExtensions
    {
        #region When

        public static Validator<T> For<T, TValue>
        (
            this Validator<T> validator,
            Expression<Func<T, IImmutableContainer, TValue>> expression,
            Action<ValidationRuleBuilder<T>> builderAction
        )
        {
            var builder = new ValidationRuleBuilder<T>(expression);
            builderAction(builder);

            var validations = builder.Build();
            return validations.Aggregate(validator, (v, r) => v.Add(r));
        }
        
        public static Validator<T> For<T, TValue>
        (
            this Validator<T> validator,
            Expression<Func<T, TValue>> expression,
            Action<ValidationRuleBuilder<T>> builderAction
        )
        {
            var parameters = new[]
            {
                expression.Parameters.Single(),
                Expression.Parameter(typeof(IImmutableContainer), $"<{typeof(IImmutableContainer).ToPrettyString()}>")
            };

            var expr = Expression.Lambda<Func<T, IImmutableContainer, TValue>>(expression.Body, parameters);

            return validator.For(expr, builderAction);
        }

        #endregion

        #region Rule APIs

        // --- Accept

//        public static Validator<T, TContext> Accept<T, TContext>(this Validator<T, TContext> rules, BuildRuleCallback<T, TContext> builder)
//        {
//            return rules.Add(builder(ValidationRuleBuilder<T, TContext>.Empty).Build());
//        }
//
//        public static Validator<T, object> Accept<T>(this Validator<T, object> rules, BuildRuleCallback<T, object> builder)
//        {
//            return rules.Add(builder(ValidationRuleBuilder<T, object>.Empty).Build());
//        }
//
//        public static Validator<T, TContext> Accept<T, TContext, TValue>
//        (
//            this Validator<T, TContext> validator,
//            Expression<Func<T, TValue>> expression,
//            Func<Expression<Func<T, TValue>>, BuilderCallback<T, TContext, TValue>> builder
//        )
//        {
//            return default; // validator.Add(builder(expression).Build());
//        }
//
//        // --- Reject
//
//        public static Validator<T, TContext> Reject<T, TContext>(this Validator<T, TContext> rules, BuildRuleCallback<T, TContext> builder)
//        {
//            return rules.Add(builder(ValidationRuleBuilder<T, TContext>.Empty).Negate().Build());
//        }
//
//        public static Validator<T, object> Reject<T>(this Validator<T, object> rules, BuildRuleCallback<T, object> builder)
//        {
//            return rules.Add(builder(ValidationRuleBuilder<T, object>.Empty).Negate().Build());
//        }

        #endregion

        [NotNull]
        public static ValidationResultCollection<T> ValidateWith<T>(this T obj, Validator<T> rules, IImmutableContainer context)
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
        public static ValidationResultCollection<T> ValidateWith<T>(this T obj, Validator<T> rules)
        {
            return obj.ValidateWith(rules, ImmutableContainer.Empty);
        }

        private static IEnumerable<IValidationResult> Evaluate<T>(this Validator<T> rules, T obj, IImmutableContainer context)
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