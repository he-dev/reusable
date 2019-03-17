using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Collections;
using Reusable.Utilities.JsonNet;
using Reusable.Utilities.JsonNet.Annotations;

namespace Reusable.Flexo
{
    public interface ISwitchable
    {
        [DefaultValue(true)]
        bool Enabled { get; }
    }

    public interface IExtendable
    {
        List<IExpression> This { get; }
    }

    [UsedImplicitly]
    [PublicAPI]
    public interface IExpression : ISwitchable, IExtendable
    {
        [NotNull]
        SoftString Name { get; }

        [NotNull]
        IExpressionContext Context { get; }

        [NotNull]
        IExpression Invoke([NotNull] IExpressionContext context);
    }

    public interface IExtension<T>
    {
        //IExpression This { get; }
    }

    [Namespace("Flexo")]
    public abstract class Expression : IExpression
    {
        // ReSharper disable RedundantNameQualifier - Use full namespace to avoid conflicts with other types.
        public static readonly Type[] Types =
        {
            typeof(Reusable.Flexo.ObjectEqual),
            typeof(Reusable.Flexo.StringEqual),
            typeof(Reusable.Flexo.GreaterThan),
            typeof(Reusable.Flexo.GreaterThanOrEqual),
            typeof(Reusable.Flexo.LessThan),
            typeof(Reusable.Flexo.LessThanOrEqual),
            typeof(Reusable.Flexo.Not),
            typeof(Reusable.Flexo.All),
            typeof(Reusable.Flexo.Any),
            typeof(Reusable.Flexo.String),
            typeof(Reusable.Flexo.IIf),
            typeof(Reusable.Flexo.Switch),
            typeof(Reusable.Flexo.SwitchToDouble),
            typeof(Reusable.Flexo.GetContextItem),
            typeof(Reusable.Flexo.Contains),
            typeof(Reusable.Flexo.Matches),
            typeof(Reusable.Flexo.Min),
            typeof(Reusable.Flexo.Max),
            typeof(Reusable.Flexo.Count),
            typeof(Reusable.Flexo.Sum),
            typeof(Reusable.Flexo.ToList),
            typeof(Reusable.Flexo.Constant<>),
            typeof(Reusable.Flexo.Double),
            typeof(Reusable.Flexo.Integer),
            typeof(Reusable.Flexo.Decimal),
            typeof(Reusable.Flexo.DateTime),
            typeof(Reusable.Flexo.TimeSpan),
            typeof(Reusable.Flexo.True),
            typeof(Reusable.Flexo.False),
            typeof(Reusable.Flexo.Collection),
        };
        // ReSharper restore RedundantNameQualifier

        protected Expression(SoftString name) : this(name, ExpressionContext.Empty) { }

        protected Expression(SoftString name, IExpressionContext context)
        {
            Name = name;
            Context = context;
        }

        public virtual SoftString Name { get; }

        public IExpressionContext Context { get; }

        public bool Enabled { get; set; } = true;

        public List<IExpression> This { get; set; } = new List<IExpression>();

        public virtual IExpression Invoke(IExpressionContext context)
        {
            var extensions = This ?? Enumerable.Empty<IExpression>();
            var result = InvokeCore(context);
            return
                extensions
                    .Enabled()
                    .Aggregate
                    (
                        result,
                        (current, next) => next.Invoke(context.Set(Item.For<IExtensionContext>(), x => x.Input, current))
                    );
        }

        protected abstract IExpression InvokeCore(IExpressionContext context);
    }

    public interface IExtensionContext
    {
        IExpression Input { get; }
    }

    [PublicAPI]
    public abstract class PredicateExpression : Expression
    {
        protected PredicateExpression(string name) : base(name) { }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            //using (context.Scope(this))
            {
                return Constant.FromResult(Name, Calculate(context));
            }
        }

        protected abstract InvokeResult<bool> Calculate(IExpressionContext context);
    }

    [PublicAPI]
    public abstract class AggregateExpression : Expression, IExtension<List<IExpression>>
    {
        private readonly Func<IEnumerable<double>, double> _aggregate;

        protected AggregateExpression(string name, [NotNull] Func<IEnumerable<double>, double> aggregate) : base(name) => _aggregate = aggregate;

        public List<IExpression> Values { get; set; }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            var value = context.Input().ValueOrDefault<List<IExpression>>() ?? Values;
            var result = _aggregate(value.Enabled().Select(e => e.Invoke(context)).Values<object>().Select(v => (double)v));
            return Constant.FromValue(Name, result);
        }
    }

    [PublicAPI]
    public abstract class ComparerExpression : Expression
    {
        private readonly Func<int, bool> _predicate;

        protected ComparerExpression(string name, [NotNull] Func<int, bool> predicate) : base(name) => _predicate = predicate;

        [JsonRequired]
        public IExpression Left { get; set; }

        [JsonRequired]
        public IExpression Right { get; set; }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            var x = Left.Invoke(context);
            var y = Right.Invoke(context);

            var result = default(int);

            var compared =
                TryCompare<int>(x, y, out result) ||
                TryCompare<float>(x, y, out result) ||
                TryCompare<double>(x, y, out result) ||
                TryCompare<string>(x, y, out result) ||
                TryCompare<decimal>(x, y, out result) ||
                TryCompare<System.DateTime>(x, y, out result) ||
                TryCompare<System.TimeSpan>(x, y, out result) ||
                TryCompare<object>(x, y, out result);

            if (compared)
            {
                return Constant.FromValue(Name, _predicate(result));
            }

            throw new InvalidOperationException($"Expressions '{x.Name}' & '{y.Name}' are not comparable.");
        }

        private static bool TryCompare<T>(IExpression x, IExpression y, out int result)
        {
            if (x is Constant<T> && y is Constant<T>)
            {
                result = ComparerFactory<IExpression>.Create(c => c.ValueOrDefault<T>()).Compare(x, y);
                return true;
            }
            else
            {
                result = default;
                return false;
            }
        }
    }

    public class LambdaExpression : Expression
    {
        private readonly Func<IExpressionContext, IExpression> _invoke;

        public LambdaExpression(string name, Func<IExpressionContext, IExpression> invoke) : base(name)
        {
            _invoke = invoke;
        }

        protected override IExpression InvokeCore(IExpressionContext context)
        {
            return _invoke(context);
        }

        public static LambdaExpression Predicate(Func<IExpressionContext, InvokeResult<bool>> predicate)
        {
            return new LambdaExpression(nameof(Predicate), context => Constant.FromResult(nameof(Predicate), predicate(context)));
        }

        public static LambdaExpression Double(Func<IExpressionContext, InvokeResult<double>> calculate)
        {
            return new LambdaExpression(nameof(Double), context => Constant.FromResult(nameof(Double), calculate(context)));
        }
    }

    public readonly struct InvokeResult<T>
    {
        public InvokeResult(T value, IExpressionContext context)
        {
            Value = value;
            Context = context;
        }

        public T Value { get; }

        public IExpressionContext Context { get; }

        public static implicit operator InvokeResult<T>((T Value, IExpressionContext Context) t) => new InvokeResult<T>(t.Value, t.Context);
    }

    public static class InvokeResult
    {
        public static InvokeResult<TValue> From<TValue>(TValue value, IExpressionContext context)
        {
            return new InvokeResult<TValue>(value, context);
        }
    }
}