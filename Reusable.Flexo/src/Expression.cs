using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Collections;

namespace Reusable.Flexo
{
    public interface ISwitchable
    {
        [DefaultValue(true)]
        bool Enabled { get; }
    }

    [UsedImplicitly]
    public interface IExpression : ISwitchable
    {
        [NotNull]
        string Name { get; }

        IExpressionContext Context { get; }

        [NotNull]
        IExpression Invoke([NotNull] IExpressionContext context);
    }

    public abstract class Expression : IExpression
    {
        protected Expression(string name) => Name = name;

        protected Expression(string name, IExpressionContext context)
        {
            Name = name;
            Context = context;
        }

        public virtual string Name { get; }

        public IExpressionContext Context { get; }

        public bool Enabled { get; set; } = true;

        public abstract IExpression Invoke(IExpressionContext context);
    }

    [PublicAPI]
    public abstract class PredicateExpression : Expression
    {
        protected PredicateExpression(string name) : base(name) { }

        public override IExpression Invoke(IExpressionContext context)
        {
            using (context.Scope(this))
            {
                return Constant.FromResult(Name, Calculate(context));
            }
        }

        protected abstract InvokeResult<bool> Calculate(IExpressionContext context);
    }

    [PublicAPI]
    public abstract class AggregateExpression : Expression
    {
        private readonly Func<IEnumerable<double>, double> _aggregate;

        protected AggregateExpression(string name, [NotNull] Func<IEnumerable<double>, double> aggregate) : base(name) => _aggregate = aggregate;

        [JsonRequired]
        public IEnumerable<IExpression> Values { get; set; }

        public override IExpression Invoke(IExpressionContext context)
        {
            return Constant.FromValue(Name, _aggregate(Values.InvokeWithValidation(context).Values<double>()));
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

        public override IExpression Invoke(IExpressionContext context)
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
                TryCompare<DateTime>(x, y, out result) ||
                TryCompare<TimeSpan>(x, y, out result) ||
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
        
        public override IExpression Invoke(IExpressionContext context)
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

    public interface IParameterAttribute
    {
        string Name { get; }

        bool Required { get; }
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