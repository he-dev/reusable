using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Flawless.ExpressionVisitors;

namespace Reusable.Flawless
{
    public delegate bool ValidationPredicate<in T, in TContext>(T obj, TContext context);

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

        public IValidationResult Evaluate(T obj, TContext context)
        {
            return CreateResult(_predicate(obj, context), ToString(), _message(obj, context));
        }

        protected abstract IValidationResult CreateResult(bool success, string expression, string message);

        public override string ToString() => _expressionString;

        public static implicit operator string(ValidationRule<T, TContext> rule) => rule?.ToString();

        #region Factories

        public static ValidationRuleBuilder<T, TContext> Ensure
        {
            get { return new ValidationRuleBuilder<T, TContext>((predicate, message) => new Soft<T, TContext>(predicate, message)); }
        }

        public static ValidationRuleBuilder<T, TContext> Require
        {
            get { return new ValidationRuleBuilder<T, TContext>((predicate, message) => new Hard<T, TContext>(predicate, message)); }
        }

        #endregion
    }

    public class Hard<T, TContext> : ValidationRule<T, TContext>
    {
        public Hard
        (
            [NotNull] Expression<ValidationPredicate<T, TContext>> predicate,
            [NotNull] Expression<MessageCallback<T, TContext>> message
        ) : base(predicate, message) { }

        protected override IValidationResult CreateResult(bool success, string expression, string message)
        {
            return
                success
                    ? (IValidationResult)new Information(expression, message)
                    : (IValidationResult)new Error(expression, message);
        }
    }

    public class Soft<T, TContext> : ValidationRule<T, TContext>
    {
        public Soft
        (
            [NotNull] Expression<ValidationPredicate<T, TContext>> predicate,
            [NotNull] Expression<MessageCallback<T, TContext>> message
        ) : base(predicate, message) { }

        protected override IValidationResult CreateResult(bool success, string expression, string message)
        {
            return
                success
                    ? (IValidationResult)new Information(expression, message)
                    : (IValidationResult)new Warning(expression, message);
        }
    }
}