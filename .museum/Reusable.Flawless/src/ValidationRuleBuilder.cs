using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Flawless.ExpressionVisitors;

namespace Reusable.Flawless
{
    public interface IValidationRuleBuilder<out TBuilder>
    {
        TBuilder Predicate(LambdaExpression expression);
    }

    public class ValidationRuleBuilder<T, TContext>
    {
        private CreateValidationFailureCallback _createValidationFailureCallback;
        private LambdaExpression _predicate;
        private LambdaExpression _message = (Expression<Func<T, TContext, string>>)((x, c) => default);
        private IImmutableSet<string> _tags;

        public ValidationRuleBuilder()
        {
            Severity(ValidationFailureFactory.CreateWarning);
            _tags = ImmutableHashSet<string>.Empty;
        }

        public static ValidationRuleBuilder<T, TContext> Empty => new ValidationRuleBuilder<T, TContext>();

        public ValidationRuleBuilder<T, TContext> Severity(CreateValidationFailureCallback createValidationFailureCallback)
        {
            _createValidationFailureCallback = createValidationFailureCallback;
            return this;
        }

        public ValidationRuleBuilder<T, TContext> Predicate(Func<LambdaExpression, LambdaExpression> expression)
        {
            _predicate = expression(_predicate);
            return this;
        }

        public ValidationRuleBuilder<T, TContext> Message(Expression<Func<T, TContext, string>> message)
        {
            _message = message;
            return this;
        }

        public ValidationRuleBuilder<T, TContext> Tags(params string[] tags)
        {
            _tags = tags.ToImmutableHashSet(SoftString.Comparer);
            return this;
        }

        [NotNull]
        public IValidationRule<T, TContext> Build()
        {
            if (_predicate is null) throw new InvalidOperationException("Validation-rule requires you to set the rule first.");

            var parameters = new[]
            {
                _predicate.Parameters.ElementAtOrDefault(0) ?? ValidationParameterPrettifier.CreatePrettyParameter<T>(),
                _predicate.Parameters.ElementAtOrDefault(1) ?? ValidationParameterPrettifier.CreatePrettyParameter<TContext>()
            };

            var expressionWithParameter = parameters.Aggregate(_predicate.Body, ValidationParameterInjector.InjectParameter);
            var predicate = Expression.Lambda<ValidationPredicate<T, TContext>>(expressionWithParameter, parameters);

            var messageWithParameter = parameters.Aggregate(_message.Body, ValidationParameterInjector.InjectParameter);
            var message = Expression.Lambda<MessageCallback<T, TContext>>(messageWithParameter, parameters);

            return new ValidationRule<T, TContext>(_tags, predicate, message, _createValidationFailureCallback);
        }
    }
}