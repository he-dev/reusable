using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Configuration;
using System.Linq;
using System.Linq.Custom;
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

    public interface IValidationRule<in T, in TContext>
    {
        IImmutableSet<string> Tags { get; }

        [NotNull]
        IValidationResult Evaluate([CanBeNull] T obj, TContext context);
    }

    public class ValidationRule<T, TContext> : IValidationRule<T, TContext>
    {
        [NotNull]
        private readonly CreateValidationFailureCallback _createValidationFailure;
        private readonly ValidationPredicate<T, TContext> _evaluate;
        private readonly MessageCallback<T, TContext> _createMessage;
        private readonly string _expressionString;

        public ValidationRule
        (
            [NotNull] IImmutableSet<string> tags,
            [NotNull] Expression<ValidationPredicate<T, TContext>> predicate,
            [NotNull] Expression<MessageCallback<T, TContext>> message,
            [NotNull] CreateValidationFailureCallback createValidationFailure
        )
        {
            if (tags == null) throw new ArgumentNullException(nameof(tags));
            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
            if (message == null) throw new ArgumentNullException(nameof(message));
            if (createValidationFailure == null) throw new ArgumentNullException(nameof(createValidationFailure));

            Tags = tags;
            _evaluate = predicate.Compile();
            _createMessage = message.Compile();
            _expressionString = ValidationParameterPrettifier.Prettify<T>(predicate).ToString();
            _createValidationFailure = createValidationFailure;
        }

        public IImmutableSet<string> Tags { get; }

        public IValidationResult Evaluate(T obj, TContext context)
        {
            if (_evaluate(obj, context) is var success && success)
            {
                return new ValidationSuccess(_expressionString, Tags, _createMessage(obj, context));
            }
            else
            {
                return _createValidationFailure(_expressionString, Tags, _createMessage(obj, context));
            }
        }

        public override string ToString() => _expressionString;

        public static implicit operator string(ValidationRule<T, TContext> rule) => rule?.ToString();
    }
}