using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Data;

namespace Reusable.Flawless
{
    public delegate bool ValidationPredicate<in T, in TContext>(T obj, TContext context);

    public delegate string MessageCallback<in T, in TContext>(T obj, TContext context);

    public interface IValidationRule<T, in TContext>
    {
        ValidationRuleOption Option { get; }

        IValidationResult<T> Evaluate([CanBeNull] T obj, TContext context);
    }

    public class ValidationRuleOption : Option<ValidationRuleOption>
    {
        public ValidationRuleOption(SoftString name, IImmutableSet<SoftString> values) : base(name, values) { }

        /// <summary>
        /// Indicates that a validation is independent.
        /// </summary>
        public static readonly ValidationRuleOption Ensure = CreateWithCallerName();

        /// <summary>
        /// Indicates that a validation must succeed before other validation can be evaluated.
        /// </summary>
        public static readonly ValidationRuleOption Require = CreateWithCallerName();
    }

    internal class ValidationRule<T, TContext> : IValidationRule<T, TContext>
    {
        private readonly ValidationPredicate<T, TContext> _predicate;
        private readonly MessageCallback<T, TContext> _message;
        private readonly string _expressionString;

        public ValidationRule
        (
            [NotNull] Expression<ValidationPredicate<T, TContext>> predicate,
            [NotNull] Expression<MessageCallback<T, TContext>> message,
            [NotNull] ValidationRuleOption option
        )
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            _predicate = predicate.Compile();
            _message = message.Compile();
            _expressionString = ValidationParameterPrettifier.Prettify<T>(predicate).ToString();
            Option = option;
        }

        public ValidationRuleOption Option { get; }

        public IValidationResult<T> Evaluate(T obj, TContext context)
        {
            return new ValidationResult<T>(ToString(), _predicate(obj, context), _message(obj, context));
        }

        public override string ToString() => _expressionString;

        public static implicit operator string(ValidationRule<T, TContext> rule) => rule?.ToString();
    }

    public static class ValidationRule
    {
        public static ValidationRuleBuilder Ensure => new ValidationRuleBuilder(ValidationRuleOption.Ensure);

        public static ValidationRuleBuilder Require => new ValidationRuleBuilder(ValidationRuleOption.Require);
    }
}