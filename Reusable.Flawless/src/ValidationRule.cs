using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Flawless.ExpressionVisitors;

namespace Reusable.Flawless
{
    public delegate bool ValidationPredicate<in T, in TContext>(T obj, TContext context);

    [CanBeNull]
    public delegate string MessageCallback<in T, in TContext>(T obj, TContext context);

    public delegate IValidationRule<T, TContext> CreateValidationRuleCallback<T, TContext>
    (
        [NotNull] Expression<ValidationPredicate<T, TContext>> predicate,
        [NotNull] Expression<MessageCallback<T, TContext>> message
    );

    public interface IValidationRule<in T, in TContext>
    {
        IValidationResult Evaluate([CanBeNull] T obj, TContext context);
    }

    public abstract class ValidationRule<T, TContext> : IValidationRule<T, TContext>
    {
        private readonly ValidationPredicate<T, TContext> _predicate;
        private readonly MessageCallback<T, TContext> _message;
        private readonly string _expressionString;

        protected ValidationRule
        (
            [NotNull] Expression<ValidationPredicate<T, TContext>> predicate,
            [NotNull] Expression<MessageCallback<T, TContext>> message
        )
        {
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));

            _predicate = predicate.Compile();
            _message = message.Compile();
            _expressionString = ValidationParameterPrettifier.Prettify<T>(predicate).ToString();
        }
        
        protected abstract string Name { get; }

        public IValidationResult Evaluate(T obj, TContext context)
        {
            return CreateResult
            (
                _predicate(obj, context) is var result && result,
                _expressionString,
                _message(obj, context) ?? $"<{typeof(T).ToPrettyString()}> {(result ? "meets" : "does not meet")} this {Name} rule."
            );
        }

        protected abstract IValidationResult CreateResult(bool success, string expression, string message);

        public override string ToString() => _expressionString;

        public static implicit operator string(ValidationRule<T, TContext> rule) => rule?.ToString();

        #region Factories

        internal static CreateValidationRuleCallback<T, TContext> Soft => (predicate, message) => new Soft<T, TContext>(predicate, message);

        internal static CreateValidationRuleCallback<T, TContext> Hard => (predicate, message) => new Hard<T, TContext>(predicate, message);

        #endregion
    }

    public class Hard<T, TContext> : ValidationRule<T, TContext>
    {
        public Hard
        (
            [NotNull] Expression<ValidationPredicate<T, TContext>> predicate,
            [NotNull] Expression<MessageCallback<T, TContext>> message
        ) : base(predicate, message) { }

        protected override string Name => nameof(Hard<T, TContext>);

        protected override IValidationResult CreateResult(bool success, string expression, string message)
        {
            return
                success
                    ? (IValidationResult)new ValidationSuccess(expression, message)
                    : (IValidationResult)new ValidationError(expression, message);
        }
    }

    public class Soft<T, TContext> : ValidationRule<T, TContext>
    {
        public Soft
        (
            [NotNull] Expression<ValidationPredicate<T, TContext>> predicate,
            [NotNull] Expression<MessageCallback<T, TContext>> message
        ) : base(predicate, message) { }

        protected override string Name => nameof(Soft<T, TContext>);
        
        protected override IValidationResult CreateResult(bool success, string expression, string message)
        {
            return
                success
                    ? (IValidationResult)new ValidationSuccess(expression, message)
                    : (IValidationResult)new ValidationWarning(expression, message);
        }
    }
}