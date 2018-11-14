using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Flexo.Extensions;

namespace Reusable.Flexo.Expressions
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

        [NotNull]
        IExpression Invoke([NotNull] IExpressionContext context);
    }

    public abstract class Expression : IExpression
    {
        protected Expression(string name) => Name = name;

        public static IExpression Empty { get; } = new Empty();

        public virtual string Name { get; }

        public bool Enabled { get; set; } = true;

        public abstract IExpression Invoke(IExpressionContext context);
    }

    public abstract class PredicateExpression : Expression
    {
        protected PredicateExpression(string name) : base(name) { }

        public override IExpression Invoke(IExpressionContext context)
        {
            using (context.Scope(this))
            {
                return Constant.Create(Name, Calculate(context)).Log();
            }
        }

        protected abstract bool Calculate(IExpressionContext context);
    }    

    public abstract class AggregateExpression : Expression
    {
        private readonly Func<IEnumerable<double>, double> _aggregate;

        protected AggregateExpression(string name, [NotNull] Func<IEnumerable<double>, double> aggregate) : base(name) => _aggregate = aggregate;

        public IEnumerable<IExpression> Expressions { get; set; }

        public override IExpression Invoke(IExpressionContext context) => Constant.Create(Name, _aggregate(Expressions.InvokeWithValidation(context).Values<double>().ToList()));
    }

    public abstract class ComparerExpression : Expression
    {
        private readonly Func<int, bool> _predicate;

        protected ComparerExpression(string name, [NotNull] Func<int, bool> predicate) : base(name) => _predicate = predicate;

        public IExpression Expression1 { get; set; }

        public IExpression Expression2 { get; set; }

        public override IExpression Invoke(IExpressionContext context)
        {
            var result1 = Expression1.InvokeWithValidation(context);
            var result2 = Expression2.InvokeWithValidation(context);

            // optimizations

            if (result1 is Constant<double> d1 && result2 is Constant<double> d2) return Constant.Create(Name, _predicate(d1.Value.CompareTo(d2.Value)));
            if (result1 is Constant<int> i1 && result2 is Constant<int> i2) return Constant.Create(Name, _predicate(i1.Value.CompareTo(i2.Value)));

            // fallback to weak comparer
            var x = (result1 as IConstant)?.Value as IComparable ?? throw new InvalidOperationException($"{nameof(Expression1)} must return an {nameof(IConstant)} expression with an {nameof(IComparable)} value.");
            var y = (result2 as IConstant)?.Value as IComparable ?? throw new InvalidOperationException($"{nameof(Expression2)} must return an {nameof(IConstant)} expression with an {nameof(IComparable)} value."); ;
            return Constant.Create(Name, _predicate(x.CompareTo(y)));
        }
    }    

    public interface IParameterAttribute
    {
        string Name { get; }

        bool Required { get; }
    }
}