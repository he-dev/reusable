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
    public delegate bool ValidationPredicate<in T>(T obj, IImmutableContainer context);

    [CanBeNull]
    public delegate string MessageCallback<in T>(T obj, IImmutableContainer context);

    public interface IValidationRule<in T>
    {
        IImmutableSet<string> Tags { get; }

        [NotNull]
        IValidationResult Evaluate([CanBeNull] T obj, IImmutableContainer context);
    }

    public class ValidationRule<T> : IValidationRule<T>
    {
        [NotNull]
        private readonly CreateValidationFailureCallback _createValidationFailure;
        private readonly ValidationPredicate<T> _evaluate;
        private readonly MessageCallback<T> _createMessage;
        private readonly string _expressionString;

        public ValidationRule
        (
            [NotNull] IImmutableSet<string> tags,
            [NotNull] Expression<ValidationPredicate<T>> predicate,
            [NotNull] Expression<MessageCallback<T>> message,
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
        
        //public static ValidationRuleBuilder<T, TContext> Builder => new ValidationRuleBuilder<T, TContext>();

        public IImmutableSet<string> Tags { get; }

        public IValidationResult Evaluate(T obj, IImmutableContainer context)
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

        public static implicit operator string(ValidationRule<T> rule) => rule?.ToString();
    }
}