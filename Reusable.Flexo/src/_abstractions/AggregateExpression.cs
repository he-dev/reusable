using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Flexo
{
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
}