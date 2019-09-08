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
    public delegate bool ValidateDelegate<in T>(T obj, IImmutableContainer context);

    [CanBeNull]
    public delegate string MessageCallback<in T>(T obj, IImmutableContainer context);

    public class ValidationRule<T> : IValidator<T>
    {
        [NotNull]
        private readonly CreateValidationFailureCallback _createValidationFailure;

        private readonly ValidateDelegate<T> _evaluate;
        private readonly MessageCallback<T> _createMessage;
        private readonly string _expressionString;

        public ValidationRule
        (
            [NotNull] IImmutableSet<string> tags,
            [NotNull] Expression<ValidateDelegate<T>> predicate,
            [NotNull] Expression<MessageCallback<T>> message,
            [NotNull] CreateValidationFailureCallback createValidationFailure
        )
        {
//            if (tags == null) throw new ArgumentNullException(nameof(tags));
//            if (predicate == null) throw new ArgumentNullException(nameof(predicate));
//            if (message == null) throw new ArgumentNullException(nameof(message));
//            if (createValidationFailure == null) throw new ArgumentNullException(nameof(createValidationFailure));

            Tags = tags;
            _evaluate = predicate.Compile();
            _createMessage = message.Compile();
            _expressionString = ValidationParameterPrettifier.Prettify<T>(predicate).ToString();
            _createValidationFailure = createValidationFailure;
        }

        public IImmutableSet<string> Tags { get; }

        public IEnumerable<IValidationResult> Validate(T obj, IImmutableContainer context)
        {
            if (_evaluate(obj, context) is var success && success)
            {
                yield return new ValidationSuccess(_expressionString, Tags, _createMessage(obj, context));
            }
            else
            {
                yield return _createValidationFailure(_expressionString, Tags, _createMessage(obj, context));
            }
        }

        public override string ToString() => _expressionString;

        public static implicit operator string(ValidationRule<T> rule) => rule?.ToString();
    }
}