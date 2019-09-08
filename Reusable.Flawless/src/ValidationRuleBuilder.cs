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

    public interface IValidationRuleBuilder : ICollection<IValidationRuleBuilder>
    {
        LambdaExpression ValueExpression { get; }

        IEnumerable<IValidator<T>> Build<T>();
    }

    public class ValidationRuleBuilder<TValue> : List<IValidationRuleBuilder>, IValidationRuleBuilder
    {
        private bool _negate;
        private readonly IList<Item> _items;
        private readonly IValidationRuleBuilder _parent;

        // expression: (value, context) => expr
        public ValidationRuleBuilder(IValidationRuleBuilder parent, LambdaExpression expression)
        {
            _parent = parent;
            ValueExpression = expression;
            _items = new List<Item>();
            parent?.Add(this);
            //_tags = ImmutableHashSet<string>.Empty;
        }

        public LambdaExpression ValueExpression { get; }

        public ValidationRuleBuilder<TValue> Not()
        {
            _negate = true;
            return this;
        }

        public ValidationRuleBuilder<TValue> Predicate(Func<LambdaExpression, LambdaExpression> createPredicate)
        {
            var predicate = createPredicate(ValueExpression);

            _items.Add(new Item
            {
                Predicate = _negate ? exprfac.Not(predicate) : predicate,
                Message = (Expression<Func<TValue, IImmutableContainer, string>>)((x, c) => default)
            });

            _negate = false;

            return this;
        }

        public ValidationRuleBuilder<TValue> Error()
        {
            _items.Last().CreateValidationFailureCallback = ValidationFailureFactory.CreateError;
            return this;
        }

        public ValidationRuleBuilder<TValue> Message(Expression<Func<TValue, string>> message)
        {
            //_rules.Last().Message = message;
            //_message = message;
            return this;
        }

        public ValidationRuleBuilder<TValue> Tags(params string[] tags)
        {
            _items.Last().Tags = tags.ToImmutableHashSet(SoftString.Comparer);
            return this;
        }

        [NotNull]
        public IEnumerable<IValidator<T>> Build<T>()
        {
            //ValidationParameterInjector.InjectParameter(_expression, Expression.Parameter())

            // (x, ctx) => x.FirstName -->
            var rules =
                from x in _items
//                let parameters = new[]
//                {
//                    _parent is null ?  x.Predicate.Parameters[0] : _parent.ValueExpression.Parameters[0], // ?? ValidationParameterPrettifier.CreatePrettyParameter<T>(),
//                    _parent is null ?  x.Predicate.Parameters[1] : _parent.ValueExpression.Parameters[1]  // ?? ValidationParameterPrettifier.CreatePrettyParameter<IImmutableContainer>()
//                }
                let expressionWithParameter = x.Predicate.Parameters.Aggregate(x.Predicate.Body, ValidationParameterInjector.InjectParameter)
                //let predicate = Expression.Lambda<ValidateDelegate<TValue>>(expressionWithParameter, x.Predicate.Parameters)
                let predicate = Expression.Lambda<ValidateDelegate<T>>(expressionWithParameter, x.Predicate.Parameters)
                //let predicate = Expression.Lambda(expressionWithParameter, x.Predicate.Parameters)
                let messageWithParameter = x.Predicate.Parameters.Aggregate(x.Message.Body, ValidationParameterInjector.InjectParameter)
                let message = Expression.Lambda<MessageCallback<T>>(messageWithParameter, x.Predicate.Parameters)
                select (IValidator<T>)new ValidationRule<T>(ImmutableHashSet<string>.Empty, predicate, message, x.CreateValidationFailureCallback);

            rules = rules.ToList();

            var sub = this.SelectMany(b => b.Build<T>());

            foreach (var rule in rules.Concat(sub))
            {
                yield return rule; //();
            }
        }

        private class Item
        {
            public LambdaExpression Predicate { get; set; }

            public LambdaExpression Message { get; set; }

            public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

            public CreateValidationFailureCallback CreateValidationFailureCallback { get; set; } = ValidationFailureFactory.CreateWarning;
        }
    }

    public static class ValidatorBuilderExtensions
    {
        public static ValidationRuleBuilder<TValue> Validate<T, TValue>(this ValidationRuleBuilder<T> builder, Expression<Func<T, TValue>> expression)
        {
            var injected = ObjectInjector.Inject(expression, builder.ValueExpression.Body);

            var lambda =
                Expression.Lambda(
                    injected,
                    builder.ValueExpression.Parameters);
            return new ValidationRuleBuilder<TValue>(builder, lambda);
        }

//        public static ValidationRuleBuilder<TValue> ValidateSelf<TValue>(this ValidationRuleBuilder<TValue> rules)
//        {
//            return rules.Validate(x => x);
//        }

        public static void Validate<T, TValue>(this ValidationRuleBuilder<T> parent, Expression<Func<T, TValue>> expression, Action<ValidationRuleBuilder<TValue>> configureBuilder)
        {
            configureBuilder(new ValidationRuleBuilder<TValue>(parent, expression.AddContextParameterIfNotExists<T, TValue>()));
        }

        private static LambdaExpression AddContextParameterIfNotExists<T, TValue>(this LambdaExpression expression)
        {
            return
                expression.Parameters.Count == 2
                    ? expression
                    : Expression.Lambda<Func<T, IImmutableContainer, TValue>>(
                        expression.Body,
                        expression.Parameters.Single(),
                        Expression.Parameter(typeof(IImmutableContainer), "context"));
        }
    }

    public class ObjectInjector : ExpressionVisitor
    {
        private readonly Expression _firstParameter;
        private readonly Expression _inject;

        private ObjectInjector(Expression firstParameter, Expression inject)
        {
            _firstParameter = firstParameter;
            _inject = inject;
        }

        public static Expression Inject(LambdaExpression lambda, Expression inject)
        {
            return new ObjectInjector(lambda.Parameters[0], inject).Visit(lambda.Body);
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            return
                node.Expression == _firstParameter
                    ? Expression.MakeMemberAccess(_inject, node.Member)
                    : base.VisitMember(node);
        }
    }
}