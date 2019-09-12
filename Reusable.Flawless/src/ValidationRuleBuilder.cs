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

    public delegate TResult ValidationFunc<in T, out TResult>(T obj, IImmutableContainer context);

    //public delegate Expression<Func<bool>> CreatePredicateFunc<in TValue>(TValue value, IImmutableContainer context);

    //public delegate Expression<Func<TValue, bool>> CreatePredicateFunc<T, TValue>(Expression<ValidationFunc<TValue, TValue>> getValue);

    public interface IValidationRuleBuilder<T> : ICollection<IValidationRuleBuilder<T>>
    {
        LambdaExpression Selector { get; }

        IEnumerable<IValidator<T>> Build();
    }

    public interface IValidationRuleBuilder<T, TValue> : IValidationRuleBuilder<T>
    {
        IValidationRuleBuilder<T, TValue> Predicate(Expression<Func<TValue, bool>> predicate);
    }

    public class ValidationRuleBuilder<T, TValue> : List<IValidationRuleBuilder<T>>, IValidationRuleBuilder<T> //, IValidationRuleBuilder<TValue>
    {
        //private readonly IValidationRuleBuilder<T> _parent;
        private readonly Expression<ValidationFunc<T, TValue>> _selector;
        private Expression<ValidationFunc<T, bool>> _when;
        private bool _negate;
        private readonly IList<Item> _items;

        private ValidationRuleBuilder()
        {
            _items = new List<Item>();
        }

        public ValidationRuleBuilder(IValidationRuleBuilder<T> parent, Expression<ValidationFunc<T, TValue>> selector) : this()
        {
            //_parent = parent;
            //_parent?.Add(this);

            //var validate = selector.AddContextParameterIfNotExists<T, TValue>();
            //var injected = ObjectInjector.Inject(validate, parent.Selector.Body);
            //_selector = Expression.Lambda<ValidationFunc<T, TValue>>(injected, parent.Selector.Parameters);
            //_selector = selector;
            var method = ((ValidationFunc<TValue, TValue>)((x, context) => x));
            var parameters = new[]
            {
                Expression.Parameter(typeof(TValue), "x"),
                Expression.Parameter(typeof(IImmutableContainer), "ctx")
            };

            _selector = selector;

            // _selector = 
            //     Expression.Lambda<ValidationFunc<TValue, TValue>>(
            //         Expression.Call(
            //             Expression.Constant(method.Target),
            //             method.Method,
            //             parameters.OfType<Expression>().ToArray()),
            //         parameters);
        }

        public static ValidationRuleBuilder<T, TNext> Create<TNext>(IValidationRuleBuilder<T> parent, Expression<Func<TValue, TNext>> selector)
        {
            var validate = selector.AddContextParameterIfNotExists<T, TNext>();
            var injected = ObjectInjector.Inject(validate, parent.Selector.Body);
            return new ValidationRuleBuilder<T, TNext>(parent, Expression.Lambda<ValidationFunc<T, TNext>>(injected, parent.Selector.Parameters));
        }

        public LambdaExpression Selector => _selector;

        public ValidationRuleBuilder<T, TValue> Not()
        {
            _negate = true;
            return this;
        }

        public ValidationRuleBuilder<T, TValue> When(Expression<Func<T, bool>> when)
        {
            //_when = when;

            var validate = when.AddContextParameterIfNotExists<T, bool>();
            var injected = ObjectInjector.Inject(validate, _selector.Body);
            _when = Expression.Lambda<ValidationFunc<T, bool>>(injected, _selector.Parameters);

            return this;
        }

        public ValidationRuleBuilder<T, TValue> Predicate(Expression<Func<TValue, bool>> predicate)
        {
            //var predicate = createPredicate(_selector);

            _items.Add(new Item
            {
                //When = _when ?? ((Expression<ValidationFunc<TValue, bool>>)((x, ctx) => true)),
                Predicate = _negate ? exprfac.Not(predicate) : predicate,
                Message = (Expression<ValidationFunc<TValue, string>>)((x, c) => default)
            });

            _negate = false;
            _when = null;

            return this;
        }

        public ValidationRuleBuilder<T, TValue> Error()
        {
            _items.Last().Required = true;
            return this;
        }

        public ValidationRuleBuilder<T, TValue> Message(LambdaExpression message)
        {
            _items.Last().Message = message;
            return this;
        }

        public ValidationRuleBuilder<T, TValue> Tags(params string[] tags)
        {
            _items.Last().Tags = tags.ToImmutableSortedSet(SoftString.Comparer);
            return this;
        }

        [NotNull]
        public IEnumerable<IValidator<T>> Build()
        {
            // There are two expressions:
            //     selector: (x, ctx) => x.FirstName
            //     predicate: x => bool;
            // that need to be called in chain like that:
            //     var value = selector(obj, context); 
            //     var result = predicate(value);
            // so I build this expression:
            //     return predicate(selector(obj, context));
            var rules =
                from item in _items
                // Need to recreate all expressions so that they use the same parameters.
                let parameters = item.Predicate.Parameters
                let when = Expression.Lambda<ValidationFunc<T, bool>>(item.When.Body, parameters)
                let predicate =
                    Expression.Lambda<ValidationFunc<T, bool>>(
                        Expression.Invoke(item.Predicate,
                            Expression.Invoke(_selector, _selector.Parameters)),
                        _selector.Parameters)
                let message = Expression.Lambda<ValidationFunc<T, string>>(item.Message.Body, parameters)
                select (IValidator<T>)new ValidationRule<T>(when, predicate, message, item.Required, ImmutableHashSet<string>.Empty);

            rules = rules.ToList();

            foreach (var rule in rules.Concat(this.SelectMany(b => b.Build())))
            {
                yield return rule;
            }
        }

        private class Item
        {
            public LambdaExpression When { get; set; }

            public Expression<Func<TValue, bool>> Predicate { get; set; }

            public LambdaExpression Message { get; set; }

            public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

            public bool Required { get; set; }
        }
    }

    public delegate TResult AggregateDelegate<TElement, TResult>(IEnumerable<TElement> source, Func<TElement, TResult> selector);

    public class CollectionValidationRuleBuilder<T, TValue> : List<IValidationRuleBuilder<T>>, IValidationRuleBuilder<T, TValue>, IValidationRuleBuilder<T>
    {
        private readonly Expression<ValidationFunc<T, IEnumerable<TValue>>> _selector;
        private readonly Expression<AggregateDelegate<TValue, bool>> _aggregate;
        //private readonly Expression<ValidationFunc<TValue, TValue>> _itemSelector;
        private LambdaExpression _when;
        private bool _negate;

        private readonly IList<Item> _items;
        //private readonly IValidationRuleBuilder<T> _parent;

        private CollectionValidationRuleBuilder()
        {
            _items = new List<Item>();
        }

        public CollectionValidationRuleBuilder(IValidationRuleBuilder<T> parent, Expression<ValidationFunc<T, IEnumerable<TValue>>> selector) : this()
        {
            //_parent = parent;
            //_parent?.Add(this);

            //var collectionSelectorWithContext = selector.AddContextParameterIfNotExists<T, IEnumerable<TValue>>();
            //var collectionSelectorFull = ObjectInjector.Inject(collectionSelectorWithContext, parent.Selector.Body);
            _selector = selector; // Expression.Lambda<ValidationFunc<T, IEnumerable<TValue>>>(collectionSelectorFull, parent.Selector.Parameters);
            var all = (AggregateDelegate<TValue, bool>)(Enumerable.All);
            var aggregateParameters = new []
            {
                Expression.Parameter(typeof(IEnumerable<TValue>), "source"),
                Expression.Parameter(typeof(Func<TValue, bool>), "predicate")
            };
            _aggregate = Expression.Lambda<AggregateDelegate<TValue, bool>>(Expression.Call(all.Method, aggregateParameters.Cast<Expression>()), aggregateParameters);

            //            var member = ((collectionCollectionSelector.Body as MemberExpression).Member as PropertyInfo).PropertyType;
            //            var interfaces =
            //                from i in member.IsInterface ? new[] { member } : member.GetInterfaces()
            //                where i.IsGenericType
            //                let gtd = i.GetGenericTypeDefinition()
            //                where typeof(IEnumerable<>).IsAssignableFrom(gtd)
            //                select i;
            //
            //            var itemType = interfaces.Single().GenericTypeArguments.Single();

            // var method = ((ValidationFunc<TValue, TValue>)((x, context) => x));
            // var parameters = new[]
            // {
            //     Expression.Parameter(typeof(TValue), "x"),
            //     Expression.Parameter(typeof(IImmutableContainer), "ctx")
            // };
            //
            // // We need a value selector for a single value not the entire collection so let's create one:
            // // (x, ctx) => x
            // _itemSelector =
            //     Expression.Lambda<ValidationFunc<TValue, TValue>>(
            //         Expression.Call(
            //             Expression.Constant(method.Target),
            //             method.Method,
            //             parameters.OfType<Expression>().ToArray()),
            //         parameters);
        }

        public LambdaExpression Selector => _selector;

        public static CollectionValidationRuleBuilder<T, TNext> Create<TNext>(IValidationRuleBuilder<T> parent, Expression<Func<TValue, IEnumerable<TNext>>> selector)
        {
            //var validate = selector.AddContextParameterIfNotExists<T, TNext>();
            //var injected = ObjectInjector.Inject(validate, parent.Selector.Body);
            var collectionSelectorWithContext = selector.AddContextParameterIfNotExists<T, IEnumerable<TNext>>();
            var collectionSelectorFull = ObjectInjector.Inject(collectionSelectorWithContext, parent.Selector.Body);
            var lambda = Expression.Lambda<ValidationFunc<T, IEnumerable<TNext>>>(collectionSelectorFull, parent.Selector.Parameters);

            return new CollectionValidationRuleBuilder<T, TNext>(parent, lambda);
        }

        public CollectionValidationRuleBuilder<T, TValue> Not()
        {
            _negate = true;
            return this;
        }

        public CollectionValidationRuleBuilder<T, TValue> When(LambdaExpression when)
        {
            _when = when;
            return this;
        }

        public IValidationRuleBuilder<T, TValue> Predicate(Expression<Func<TValue, bool>> predicate)
        {
            _items.Add(new Item
            {
                When = _when ?? ((Expression<ValidationFunc<TValue, bool>>)((x, ctx) => true)),
                Predicate = _negate ? exprfac.Not(predicate) : predicate,
                Message = (Expression<ValidationFunc<TValue, string>>)((x, c) => default)
            });

            _negate = false;
            _when = null;

            return this;
        }

        public CollectionValidationRuleBuilder<T, TValue> Error()
        {
            _items.Last().Required = true;
            return this;
        }

        public CollectionValidationRuleBuilder<T, TValue> Message(LambdaExpression message)
        {
            _items.Last().Message = message;
            return this;
        }

        public CollectionValidationRuleBuilder<T, TValue> Tags(params string[] tags)
        {
            _items.Last().Tags = tags.ToImmutableSortedSet(SoftString.Comparer);
            return this;
        }

        [NotNull]
        public IEnumerable<IValidator<T>> Build()
        {
            var getCollection = _selector.Compile();

            /*
                     
                return (T obj, TContext context) =>
                {
                    var collection = getCollection(obj, context);
                    return aggregate(collection, x => predicate(x, context));
                };
                
                as:
                
                (T obj, TContext context) => aggregate(getCollection(obj, context), x => predicate(x, context));
                                             Invoke    Invoke                       Lambda
             
             */

            // (x, ctx) => x.FirstName -->
            var rules =
                from item in _items
                // Need to recreate all expressions so that they use the same parameters.
                let parameters = item.Predicate.Parameters
                let predicate =
                    Expression.Lambda<ValidationFunc<T, bool>>(
                        Expression.Invoke(
                            _aggregate,
                            Expression.Invoke(_selector, _selector.Parameters), item.Predicate),
                        _selector.Parameters)
                let when = Expression.Lambda<ValidationFunc<T, bool>>(item.When.Body, _selector.Parameters)
                let message = Expression.Lambda<ValidationFunc<T, string>>(item.Message.Body, _selector.Parameters)
                select (IValidator<T>)new ValidationRule<T>(when, predicate, message, item.Required, ImmutableHashSet<string>.Empty);

            rules = rules.ToList();

            foreach (var rule in rules.Concat(this.SelectMany(b => b.Build())))
            {
                yield return rule;
            }
        }

        private static ValidationFunc<T, bool> CreateValidateCollectionDelegate
        (
            ValidationFunc<T, IEnumerable<TValue>> getCollection,
            AggregateDelegate<TValue, bool> aggregate,
            ValidationFunc<TValue, bool> predicate
        )
        {
            //Enumerable.All()
            return (obj, context) =>
            {
                var collection = getCollection(obj, context);
                return aggregate(collection, x => predicate(x, context));
            };
        }

        private class Item
        {
            public LambdaExpression When { get; set; }

            public Expression<Func<TValue, bool>> Predicate { get; set; }

            public LambdaExpression Message { get; set; }

            public IEnumerable<string> Tags { get; set; } = Enumerable.Empty<string>();

            public bool Required { get; set; }
        }
    }

    public static class ValidatorBuilderExtensions
    {
        public static ValidationRuleBuilder<T, TNext> Validate<T, TCurrent, TNext>(this ValidationRuleBuilder<T, TCurrent> builder, Expression<Func<TCurrent, TNext>> selector)
        {
            return ValidationRuleBuilder<T, TCurrent>.Create(builder, selector);
        }

        //        public static ValidationRuleBuilder<TValue> ValidateSelf<TValue>(this ValidationRuleBuilder<TValue> rules)
        //        {
        //            return rules.Validate(x => x);
        //        }

        public static void Validate<T, TCurrent, TNext>(this ValidationRuleBuilder<T, TCurrent> builder, Expression<Func<TCurrent, TNext>> selector, Action<ValidationRuleBuilder<T, TNext>> configureBuilder)
        {
            //var validate = expression.AddContextParameterIfNotExists<T, TValue>();
            //var injected = ObjectInjector.Inject(validate, builder.ValueSelector.Body);
            //var lambda = Expression.Lambda<ValidationFunc<T, TValue>>(injected, builder.ValueSelector.Parameters);

            configureBuilder(ValidationRuleBuilder<T, TCurrent>.Create(builder, selector));
        }

        //        public static ValidationRuleBuilder<T, IEnumerable<TValue>> ValidateItems<T, TValue>(this ValidationRuleBuilder<T, TValue> builder, Expression<Func<T, IEnumerable<TValue>>> collectionSelector)
        //        {
        //            //var validate = collectionSelector.AddContextParameterIfNotExists<T, TValue>();
        //            //var injected = ObjectInjector.Inject(validate, builder.ValueSelector.Body);
        //            //var lambda = Expression.Lambda<ValidationFunc<T, IEnumerable<TValue>>>(injected, builder.ValueSelector.Parameters);
        //
        //            //configureBuilder(new ValidationRuleBuilder<IEnumerable<TValue>>(builder, lambda));
        //            return new ValidationRuleBuilder<T, IEnumerable<TValue>>(builder, lambda);
        //        }

        public static CollectionValidationRuleBuilder<T, TNext> Validate<T, TCurrent, TNext>
        (
            this ValidationRuleBuilder<T, TCurrent> builder,
            Expression<Func<TCurrent, IEnumerable<TNext>>> selector,
            Func<IEnumerable<TNext>, Func<TNext, bool>, bool> func
        )
        {
            //var collectionSelectorWithContext = selector.AddContextParameterIfNotExists<T, IEnumerable<TValue>>();
            //var collectionSelectorFull = ObjectInjector.Inject(collectionSelectorWithContext, builder.ValueSelector.Body);
            //var lambda = Expression.Lambda<ValidationFunc<T, IEnumerable<TValue>>>(collectionSelectorFull, builder.ValueSelector.Parameters);

            //configureBuilder(new ValidationRuleBuilder<IEnumerable<TValue>>(builder, lambda));
            return CollectionValidationRuleBuilder<T, TCurrent>.Create(builder, selector);
        }

        public static CollectionValidationRuleBuilder<T, TNext> ValidateAll<T, TCurrent, TNext>
        (
            this ValidationRuleBuilder<T, TCurrent> builder,
            Expression<Func<TCurrent, IEnumerable<TNext>>> selector
        )
        {
            return builder.Validate(selector, Enumerable.All);
        }

        public static CollectionValidationRuleBuilder<T, TNext> ValidateAny<T, TCurrent, TNext>
        (
            this ValidationRuleBuilder<T, TCurrent> builder,
            Expression<Func<TCurrent, IEnumerable<TNext>>> selector
        )
        {
            return builder.Validate(selector, Enumerable.Any);
        }

        public static LambdaExpression AddContextParameterIfNotExists<T, TValue>(this LambdaExpression expression)
        {
            return
                expression.Parameters.Count == 2
                    ? expression
                    : Expression.Lambda<ValidationFunc<T, TValue>>(
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
        public static Expression<ValidationFunc<T, TValue>> Invoke(LambdaExpression lambda)
        {
            return (Expression<ValidationFunc<T, TValue>>)new ReplaceLambda<T, TValue>().Visit(lambda);
        }

        protected override Expression VisitLambda<TCurrent>(Expression<TCurrent> node)
        {
            return Expression.Lambda<ValidationFunc<T, TValue>>(Visit(node.Body), node.Parameters);
        }
    }
}