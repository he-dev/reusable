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
    //public delegate ValidationRuleBuilder<T> BuildRuleCallback<T, TContext>(ValidationRuleBuilder<T> builder);

    //public delegate ValidationRuleBuilder<T> BuilderCallback<T, TContext, TValue>(Expression<Func<T, TValue>> expression);

    [PublicAPI]
    public static class ValidatorExtensions
    {
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
        public static IEnumerable<IValidationResult> ValidateWith<T>(this T obj, Validator<T> validator, IImmutableContainer context)
        {
            return validator.Validate(obj, context);
        }

        [NotNull]
        public static IEnumerable<IValidationResult> ValidateWith<T>(this T obj, Validator<T> validator)
        {
            return obj.ValidateWith(validator, ImmutableContainer.Empty);
        }
    }
}