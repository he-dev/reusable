using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Flawless.ExpressionVisitors;

namespace Reusable.Flawless
{
    public class ValidationRuleBuilder
    {
        private readonly ValidationRuleOption _option;

        private LambdaExpression _predicate;
        private LambdaExpression _message;

        public ValidationRuleBuilder(ValidationRuleOption option)
        {
            _option = option;
        }

        public ValidationRuleBuilder Predicate(LambdaExpression expression)
        {
            _predicate = expression;
            return this;
        }

        public ValidationRuleBuilder Message(Expression<Func<string>> message)
        {
            _message = message;
            return this;
        }

        [NotNull]
        public IValidationRule<T, TContext> Build<T, TContext>()
        {
            if (_predicate is null || _message is null) throw new InvalidOperationException("Validation-rule requires you to set rule and message first.");
            
            var parameters = new[]
            {
                _predicate.Parameters.ElementAtOrDefault(0) ?? ValidationParameterPrettifier.CreatePrettyParameter<T>(),
                _predicate.Parameters.ElementAtOrDefault(1) ?? ValidationParameterPrettifier.CreatePrettyParameter<TContext>()
            };

            var expressionWithParameter = parameters.Aggregate(_predicate.Body, ValidationParameterInjector.InjectParameter);
            var predicate = Expression.Lambda<ValidationPredicate<T, TContext>>(expressionWithParameter, parameters);

            var messageWithParameter = parameters.Aggregate(_message.Body, ValidationParameterInjector.InjectParameter);
            var message = Expression.Lambda<MessageCallback<T, TContext>>(messageWithParameter, parameters);

            return new ValidationRule<T, TContext>(predicate, message, _option);
        }
    }
}