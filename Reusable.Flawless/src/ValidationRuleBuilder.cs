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
        LambdaExpression ValueExpression { get; }

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
            ValueExpression = expression;
        }

        public LambdaExpression ValueExpression { get; }

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
            var predicate = createPredicate(ValueExpression);

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

    public class CollectionValidationRuleBuilder<TValue> : List<IValidationRuleBuilder>, IValidationRuleBuilder, IValidationRuleBuilder<TValue>
    {
        private LambdaExpression _when;
        private bool _negate;
        private readonly IList<Item> _items;
        private readonly IValidationRuleBuilder _parent;
        private readonly LambdaExpression _selector;

        private CollectionValidationRuleBuilder()
        {
            _items = new List<Item>();
        }

        public CollectionValidationRuleBuilder(IValidationRuleBuilder parent, LambdaExpression expression) : this()
        {
            _parent = parent;
            _parent?.Add(this);
            _selector = expression;

            var member = ((expression.Body as MemberExpression).Member as PropertyInfo).PropertyType;
            var interfaces =
                from i in member.IsInterface ? new[] { member } : member.GetInterfaces()
                where i.IsGenericType
                let gtd = i.GetGenericTypeDefinition()
                where typeof(IEnumerable<>).IsAssignableFrom(gtd)
                select i;

            var itemType = interfaces.Single().GenericTypeArguments.Single();

            var method = ((EvaluateDelegate<TValue, TValue>)((x, _) => x));
            //var method = (Expression<Func<TValue, TValue>>)(x => x);
            var param1 = Expression.Parameter(typeof(TValue), "x");
            var param2 = Expression.Parameter(typeof(IImmutableContainer), "context");
            ValueExpression =
                Expression.Lambda<EvaluateDelegate<TValue, TValue>>(
                    Expression.Call(Expression.Constant(method.Target), method.Method, param1, param2), param1, param2);
        }

        public LambdaExpression ValueExpression { get; }

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
            var predicate = createPredicate(ValueExpression);

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
            var firstItem = _items.First();
            var evalDelegate = ValidateCollection<T>(Enumerable.All, firstItem);
            //var param2 = Expression.Parameter(typeof(IImmutableContainer), "context");
            var predicateResult =
                Expression.Lambda<EvaluateDelegate<T, bool>>(
                    Expression.Call(
                        Expression.Constant(evalDelegate.Target),
                        evalDelegate.Method,
                        _selector.Parameters),
                    _selector.Parameters);

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


        private EvaluateDelegate<T, bool> ValidateCollection<T>(Func<IEnumerable<TValue>, Func<TValue, bool>, bool> func, Item item)
        {
            var getItems = (Func<T, IImmutableContainer, IEnumerable<TValue>>)_selector.Compile();
            var predicate = (Func<TValue, IImmutableContainer, bool>)item.Predicate.Compile();

            return (obj, context) =>
            {
                var source = getItems(obj, context);
                return func(source, x => predicate(x, context));
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
            var injected = ObjectInjector.Inject(validate, builder.ValueExpression.Body);
            var lambda = Expression.Lambda(injected, builder.ValueExpression.Parameters);
            return new ValidationRuleBuilder<TValue>(builder, lambda);
        }

//        public static ValidationRuleBuilder<TValue> ValidateSelf<TValue>(this ValidationRuleBuilder<TValue> rules)
//        {
//            return rules.Validate(x => x);
//        }

        public static void Validate<T, TValue>(this ValidationRuleBuilder<T> builder, Expression<Func<T, TValue>> expression, Action<ValidationRuleBuilder<TValue>> configureBuilder)
        {
            var validate = expression.AddContextParameterIfNotExists<T, TValue>();
            var injected = ObjectInjector.Inject(validate, builder.ValueExpression.Body);
            var lambda = Expression.Lambda(injected, builder.ValueExpression.Parameters);

            configureBuilder(new ValidationRuleBuilder<TValue>(builder, lambda));
        }

        public static ValidationRuleBuilder<IEnumerable<TValue>> ValidateItems<T, TValue>(this ValidationRuleBuilder<T> builder, Expression<Func<T, IEnumerable<TValue>>> expression)
        {
            var validate = expression.AddContextParameterIfNotExists<T, TValue>();
            var injected = ObjectInjector.Inject(validate, builder.ValueExpression.Body);
            var lambda = Expression.Lambda(injected, builder.ValueExpression.Parameters);

            //configureBuilder(new ValidationRuleBuilder<IEnumerable<TValue>>(builder, lambda));
            return new ValidationRuleBuilder<IEnumerable<TValue>>(builder, lambda);
        }

        public static CollectionValidationRuleBuilder<TValue> Validate<T, TValue>
        (
            this ValidationRuleBuilder<T> builder,
            Expression<Func<T, IEnumerable<TValue>>> expression,
            Func<IEnumerable<TValue>, Func<TValue, bool>, bool> func
        )
        {
            //var validate = expression.AddContextParameterIfNotExists<T, TValue>();
            //var injected = ObjectInjector.Inject(validate, builder.ValueExpression.Body);
            //var lambda = Expression.Lambda(injected, builder.ValueExpression.Parameters);

            //configureBuilder(new ValidationRuleBuilder<IEnumerable<TValue>>(builder, lambda));
            return new CollectionValidationRuleBuilder<TValue>(builder, expression.AddContextParameterIfNotExists<T, IEnumerable<TValue>>());
        }


        public static LambdaExpression AddContextParameterIfNotExists<T, TValue>(this LambdaExpression expression)
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
}