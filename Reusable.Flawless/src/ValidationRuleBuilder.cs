using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Flawless.ExpressionVisitors;
using Reusable.Flawless.Helpers;

namespace Reusable.Flawless
{
    using exprfac = ValidationExpressionFactory;

    public interface IValidationRuleBuilder : ICollection<IValidationRuleBuilder>
    {
        /// <summary>
        /// Gets the value to validate.
        /// </summary>
        LambdaExpression ValueSelector { get; }

        IEnumerable<IValidator<T>> Build<T>();
    }

    public interface IValidationRuleBuilder<T> : IValidationRuleBuilder
    {
        IValidationRuleBuilder<T> Predicate(Func<LambdaExpression, LambdaExpression> createPredicate);
    }

    public class ValidationRuleBuilder<TValue> : List<IValidationRuleBuilder>, IValidationRuleBuilder //, IValidationRuleBuilder<TValue>
    {
        private LambdaExpression _when;
        private bool _negate;
        private readonly IList<Item> _items;
        private readonly IValidationRuleBuilder _parent;

        private ValidationRuleBuilder()
        {
            _items = new List<Item>();
        }

        public ValidationRuleBuilder(IValidationRuleBuilder parent, LambdaExpression expression) : this()
        {
            _parent = parent;
            _parent?.Add(this);
            ValueSelector = expression;
        }

        public LambdaExpression ValueSelector { get; }

        public ValidationRuleBuilder<TValue> Not()
        {
            _negate = true;
            return this;
        }

        public ValidationRuleBuilder<TValue> When(LambdaExpression when)
        {
            _when = when;
            return this;
        }

        public ValidationRuleBuilder<TValue> Predicate(Func<LambdaExpression, LambdaExpression> createPredicate)
        {
            var predicate = createPredicate(ValueSelector);

            _items.Add(new Item
            {
                When = _when ?? ((Expression<EvaluateDelegate<TValue, bool>>)((x, ctx) => true)),
                Predicate = _negate ? exprfac.Not(predicate) : predicate,
                Message = (Expression<EvaluateDelegate<TValue, string>>)((x, c) => default)
            });

            _negate = false;
            _when = null;

            return this;
        }

        public ValidationRuleBuilder<TValue> Error()
        {
            _items.Last().Required = true;
            return this;
        }

        public ValidationRuleBuilder<TValue> Message(LambdaExpression message)
        {
            _items.Last().Message = message;
            return this;
        }

        public ValidationRuleBuilder<TValue> Tags(params string[] tags)
        {
            _items.Last().Tags = tags.ToImmutableSortedSet(SoftString.Comparer);
            return this;
        }

        [NotNull]
        public IEnumerable<IValidator<T>> Build<T>()
        {
            // (x, ctx) => x.FirstName -->
            var rules =
                from item in _items
                // Need to recreate all expressions so that they use the same parameters.
                let parameters = item.Predicate.Parameters
                let when = Expression.Lambda<EvaluateDelegate<T, bool>>(item.When.Body, parameters)
                let predicate = Expression.Lambda<EvaluateDelegate<T, bool>>(item.Predicate.Body, parameters)
                let message = Expression.Lambda<EvaluateDelegate<T, string>>(item.Message.Body, parameters)
                select (IValidator<T>)new ValidationRule<T>(when, predicate, message, item.Required, ImmutableHashSet<string>.Empty);

            foreach (var rule in rules.Concat(this.SelectMany(b => b.Build<T>())))
            {
                yield return rule;
            }
        }

        private Expression<EvaluateDelegate<TValue, bool>> ValidateCollectionExpression(Func<IEnumerable<TValue>, Func<TValue, bool>, bool> func)
        {
            return default;
        }

        private EvaluateDelegate<T, bool> ValidateCollection<T>(IEnumerable<TValue> source, Func<IEnumerable<TValue>, Func<TValue, bool>, bool> func)
        {
            return (obj, context) =>
            {
                //return func(source, x => predicate());
                return default;
            };
        }

        private class Item
        {
            public LambdaExpression When { get; set; }

            public LambdaExpression Predicate { get; set; }

            public LambdaExpression Message { get; set; }

            public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

            public bool Required { get; set; }
        }
    }

    public delegate TResult AggregateDelegate<TElement, TResult>(IEnumerable<TElement> source, Func<TElement, TResult> selector);

    public class CollectionValidationRuleBuilder<TValue> : List<IValidationRuleBuilder>, IValidationRuleBuilder, IValidationRuleBuilder<TValue>
    {
        private LambdaExpression _when;
        private bool _negate;
        private readonly IList<Item> _items;
        private readonly IValidationRuleBuilder _parent;
        private readonly LambdaExpression _collectionSelector;

        private CollectionValidationRuleBuilder()
        {
            _items = new List<Item>();
        }

        public CollectionValidationRuleBuilder(IValidationRuleBuilder parent, LambdaExpression collectionCollectionSelector) : this()
        {
            _parent = parent;
            _parent?.Add(this);
            _collectionSelector = collectionCollectionSelector;

//            var member = ((collectionCollectionSelector.Body as MemberExpression).Member as PropertyInfo).PropertyType;
//            var interfaces =
//                from i in member.IsInterface ? new[] { member } : member.GetInterfaces()
//                where i.IsGenericType
//                let gtd = i.GetGenericTypeDefinition()
//                where typeof(IEnumerable<>).IsAssignableFrom(gtd)
//                select i;
//
//            var itemType = interfaces.Single().GenericTypeArguments.Single();

            var method = ((EvaluateDelegate<TValue, TValue>)((x, _) => x));
            var parameters = new[]
            {
                Expression.Parameter(typeof(TValue), "x"),
                Expression.Parameter(typeof(IImmutableContainer), "ctx")
            };

            // We need a value selector for a single value not the entire collection so let's create one:
            // (x, ctx) => x
            ValueSelector =
                Expression.Lambda<EvaluateDelegate<TValue, TValue>>(
                    Expression.Call(
                        Expression.Constant(method.Target),
                        method.Method,
                        parameters.OfType<Expression>().ToArray()),
                    parameters);
        }

        public LambdaExpression ValueSelector { get; }

        public CollectionValidationRuleBuilder<TValue> Not()
        {
            _negate = true;
            return this;
        }

        public CollectionValidationRuleBuilder<TValue> When(LambdaExpression when)
        {
            _when = when;
            return this;
        }

        public IValidationRuleBuilder<TValue> Predicate(Func<LambdaExpression, LambdaExpression> createPredicate)
        {
            var predicate = createPredicate(ValueSelector);

            _items.Add(new Item
            {
                When = _when ?? ((Expression<EvaluateDelegate<TValue, bool>>)((x, ctx) => true)),
                Predicate = _negate ? exprfac.Not(predicate) : predicate,
                Message = (Expression<EvaluateDelegate<TValue, string>>)((x, c) => default)
            });

            _negate = false;
            _when = null;

            return this;
        }

        public CollectionValidationRuleBuilder<TValue> Error()
        {
            _items.Last().Required = true;
            return this;
        }

        public CollectionValidationRuleBuilder<TValue> Message(LambdaExpression message)
        {
            _items.Last().Message = message;
            return this;
        }

        public CollectionValidationRuleBuilder<TValue> Tags(params string[] tags)
        {
            _items.Last().Tags = tags.ToImmutableSortedSet(SoftString.Comparer);
            return this;
        }

        [NotNull]
        public IEnumerable<IValidator<T>> Build<T>()
        {
            var getCollection = (EvaluateDelegate<T, IEnumerable<TValue>>)_collectionSelector.Compile();

            var firstItem = _items.First();
            var predi = ReplaceLambda<TValue, bool>.Invoke(firstItem.Predicate);
            var itemPredicate = (EvaluateDelegate<TValue, bool>)predi.Compile();
            var evalDelegate = ValidateCollection(getCollection, Enumerable.All, itemPredicate);
            var predicateResult =
                Expression.Lambda<EvaluateDelegate<T, bool>>(
                    Expression.Call(
                        Expression.Constant(evalDelegate.Target),
                        evalDelegate.Method,
                        _collectionSelector.Parameters),
                    _collectionSelector.Parameters);

            yield return (IValidator<T>)new ValidationRule<T>((x, _) => true, predicateResult, (x, _) => "asdf", firstItem.Required, ImmutableHashSet<string>.Empty);

            // (x, ctx) => x.FirstName -->
            var rules =
                from item in _items
                // Need to recreate all expressions so that they use the same parameters.
                let parameters = item.Predicate.Parameters
                let when = Expression.Lambda<EvaluateDelegate<T, bool>>(item.When.Body, parameters)
                let predicate = Expression.Lambda<EvaluateDelegate<T, bool>>(item.Predicate.Body, parameters)
                let message = Expression.Lambda<EvaluateDelegate<T, string>>(item.Message.Body, parameters)
                select (IValidator<T>)new ValidationRule<T>(when, predicate, message, item.Required, ImmutableHashSet<string>.Empty);

            foreach (var rule in rules.Concat(this.SelectMany(b => b.Build<T>())))
            {
                yield return rule;
            }
        }


        private static EvaluateDelegate<T, bool> ValidateCollection<T>
        (
            EvaluateDelegate<T, IEnumerable<TValue>> getCollection,
            AggregateDelegate<TValue, bool> aggregate,
            EvaluateDelegate<TValue, bool> predicate
        )
        {
            //var predicate = (Func<TValue, IImmutableContainer, bool>)item.Predicate.Compile();

            return (obj, context) =>
            {
                var collection = getCollection(obj, context);
                return aggregate(collection, x => predicate(x, context));
            };
        }

        private class Item
        {
            public LambdaExpression When { get; set; }

            public LambdaExpression Predicate { get; set; }

            public LambdaExpression Message { get; set; }

            public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

            public bool Required { get; set; }
        }
    }

    public static class ValidatorBuilderExtensions
    {
        public static ValidationRuleBuilder<TValue> Validate<T, TValue>(this ValidationRuleBuilder<T> builder, Expression<Func<T, TValue>> expression)
        {
            var validate = expression.AddContextParameterIfNotExists<T, TValue>();
            var injected = ObjectInjector.Inject(validate, builder.ValueSelector.Body);
            var lambda = Expression.Lambda(injected, builder.ValueSelector.Parameters);
            return new ValidationRuleBuilder<TValue>(builder, lambda);
        }

//        public static ValidationRuleBuilder<TValue> ValidateSelf<TValue>(this ValidationRuleBuilder<TValue> rules)
//        {
//            return rules.Validate(x => x);
//        }

        public static void Validate<T, TValue>(this ValidationRuleBuilder<T> builder, Expression<Func<T, TValue>> expression, Action<ValidationRuleBuilder<TValue>> configureBuilder)
        {
            var validate = expression.AddContextParameterIfNotExists<T, TValue>();
            var injected = ObjectInjector.Inject(validate, builder.ValueSelector.Body);
            var lambda = Expression.Lambda(injected, builder.ValueSelector.Parameters);

            configureBuilder(new ValidationRuleBuilder<TValue>(builder, lambda));
        }

        public static ValidationRuleBuilder<IEnumerable<TValue>> ValidateItems<T, TValue>(this ValidationRuleBuilder<T> builder, Expression<Func<T, IEnumerable<TValue>>> collectionSelector)
        {
            var validate = collectionSelector.AddContextParameterIfNotExists<T, TValue>();
            var injected = ObjectInjector.Inject(validate, builder.ValueSelector.Body);
            var lambda = Expression.Lambda(injected, builder.ValueSelector.Parameters);

            //configureBuilder(new ValidationRuleBuilder<IEnumerable<TValue>>(builder, lambda));
            return new ValidationRuleBuilder<IEnumerable<TValue>>(builder, lambda);
        }

        public static CollectionValidationRuleBuilder<TValue> Validate<T, TValue>
        (
            this ValidationRuleBuilder<T> builder,
            Expression<Func<T, IEnumerable<TValue>>> collectionSelector,
            Func<IEnumerable<TValue>, Func<TValue, bool>, bool> func
        )
        {
            var collectionSelectorWithContext = collectionSelector.AddContextParameterIfNotExists<T, IEnumerable<TValue>>();
            var collectionSelectorFull = ObjectInjector.Inject(collectionSelectorWithContext, builder.ValueSelector.Body);
            var lambda = Expression.Lambda<EvaluateDelegate<T, IEnumerable<TValue>>>(collectionSelectorFull, builder.ValueSelector.Parameters);

            //configureBuilder(new ValidationRuleBuilder<IEnumerable<TValue>>(builder, lambda));
            return new CollectionValidationRuleBuilder<TValue>(builder, lambda);
        }


        public static LambdaExpression AddContextParameterIfNotExists<T, TValue>(this LambdaExpression expression)
        {
            return
                expression.Parameters.Count == 2
                    ? expression
                    : Expression.Lambda<EvaluateDelegate<T, TValue>>(
                        expression.Body,
                        expression.Parameters.Single(),
                        Expression.Parameter(typeof(IImmutableContainer), "ctx"));
        }
    }

    // y => y.Member --> x => x.Member.Member
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

        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            return
                node.Object == _firstParameter
                    ? Expression.Call(_inject, node.Method, node.Arguments)
                    : base.VisitMethodCall(node);
        }
    }

    public class ReplaceLambda<T, TValue> : ExpressionVisitor
    {
        public static LambdaExpression Invoke(LambdaExpression lambda)
        {
            return (LambdaExpression)new ReplaceLambda<T, TValue>().Visit(lambda);
        }

        protected override Expression VisitLambda<TCurrent>(Expression<TCurrent> node)
        {
            return Expression.Lambda<EvaluateDelegate<T, TValue>>(Visit(node.Body), node.Parameters);
        }
    }
}