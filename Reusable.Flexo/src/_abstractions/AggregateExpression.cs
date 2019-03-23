using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Flexo
{
    [PublicAPI]
    public abstract class AggregateExpression : Expression<double>, IExtension<IEnumerable<object>>
    {
        private readonly Func<IEnumerable<double>, double> _aggregate;

        protected AggregateExpression(string name, [NotNull] Func<IEnumerable<double>, double> aggregate)
            : base(name) => _aggregate = aggregate;

        public IEnumerable<IExpression> Values { get; set; } = new List<IExpression>();

        protected override Constant<double> InvokeCore(IExpressionContext context)
        {
            if (context.TryPopExtensionInput(out IEnumerable<object> input))
            {
                var result = _aggregate(input.Cast<double>());
                return (Name, result, context);
            }
            else
            {
                var values = Values.Enabled().Select(e => e.Invoke(context)).Values<object>().Cast<double>();
                var result = _aggregate(values);
                return (Name, result, context);
            }
        }
    }
}