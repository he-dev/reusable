using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Flawless.ExpressionVisitors;
using Reusable.Flawless.Helpers;

namespace Reusable.Flawless
{
    using exprfac = ValidationExpressionFactory;

    public class ValidationRuleBuilder<T>
    {
        private readonly IList<Item> _items;
        private bool _not;

        private readonly LambdaExpression _expression;
        //private LambdaExpression _message = (Expression<Func<T, TContext, string>>)((x, c) => default);
        //private IImmutableSet<string> _tags;

        public ValidationRuleBuilder(LambdaExpression expression)
        {
            _expression = expression;
            //Severity(ValidationFailureFactory.CreateWarning);
            _items = new List<Item>();
            //_tags = ImmutableHashSet<string>.Empty;
        }

        public ValidationRuleBuilder<T> Not()
        {
            _not = true;
            return this;
        }

        public ValidationRuleBuilder<T> Predicate(Func<LambdaExpression, LambdaExpression> createPredicate)
        {
            var predicate = createPredicate(_expression);
            if (_not)
            {
                predicate = exprfac.Not(predicate);
                _not = false;
            }

            _items.Add(new Item { Predicate = predicate, Message = (Expression<Func<T, IImmutableContainer, string>>)((x, c) => default) });

            return this;
        }

        public ValidationRuleBuilder<T> Required()
        {
            _items.Last().CreateValidationFailureCallback = ValidationFailureFactory.CreateError;
            return this;
        }

        public ValidationRuleBuilder<T> Message(Expression<Func<T, string>> message)
        {
            //_rules.Last().Message = message;
            //_message = message;
            return this;
        }

        public ValidationRuleBuilder<T> Tags(params string[] tags)
        {
            _items.Last().Tags = tags.ToImmutableHashSet(SoftString.Comparer);
            return this;
        }

        [NotNull]
        public IList<IValidationRule<T>> Build()
        {
            var rules =
                from x in _items
//                let parameters = new[]
//                {
//                    x.Predicate.Parameters.ElementAtOrDefault(0) ?? ValidationParameterPrettifier.CreatePrettyParameter<T>(),
//                    x.Predicate.Parameters.ElementAtOrDefault(1) ?? ValidationParameterPrettifier.CreatePrettyParameter<IImmutableContainer>()
//                }
                let expressionWithParameter = x.Predicate.Parameters.Aggregate(x.Predicate.Body, ValidationParameterInjector.InjectParameter)
                let predicate = Expression.Lambda<ValidationPredicate<T>>(expressionWithParameter, x.Predicate.Parameters)
                let messageWithParameter = x.Predicate.Parameters.Aggregate(x.Message.Body, ValidationParameterInjector.InjectParameter)
                let message = Expression.Lambda<MessageCallback<T>>(messageWithParameter, x.Predicate.Parameters)
                select new ValidationRule<T>(ImmutableHashSet<string>.Empty, predicate, message, x.CreateValidationFailureCallback);

            return rules.Cast<IValidationRule<T>>().ToList();

            //if (_predicate is null) throw new InvalidOperationException("Validation-rule requires you to set the rule first.");

//            var rules =
//                from x in _items
//                let parameters = new[]
//                {
//                    x.Predicate.Parameters.ElementAtOrDefault(0) ?? ValidationParameterPrettifier.CreatePrettyParameter<T>(),
//                    x.Predicate.Parameters.ElementAtOrDefault(1) ?? ValidationParameterPrettifier.CreatePrettyParameter<IImmutableContainer>()
//                };
//                
//
//
//            var expressionWithParameter = parameters.Aggregate(_predicate.Body, ValidationParameterInjector.InjectParameter);
//            var predicate = Expression.Lambda<ValidationPredicate<T>>(expressionWithParameter, parameters);
//
//            var messageWithParameter = parameters.Aggregate(_message.Body, ValidationParameterInjector.InjectParameter);
//            var message = Expression.Lambda<MessageCallback<T>>(messageWithParameter, parameters);
//
//            return new ValidationRule<T>(_tags, predicate, message, _createValidationFailureCallback);

            return default;
        }

        private class Item
        {
            public LambdaExpression Predicate { get; set; }

            public LambdaExpression Message { get; set; }

            public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

            public CreateValidationFailureCallback CreateValidationFailureCallback { get; set; } = ValidationFailureFactory.CreateWarning;
        }
    }
}