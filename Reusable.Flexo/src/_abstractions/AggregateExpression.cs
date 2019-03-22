using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.Flexo
{
    [PublicAPI]
    public abstract class AggregateExpression : Expression<double>, IExtension<IEnumerable<IExpression>>
    {
        private readonly Func<IEnumerable<double>, double> _aggregate;

        protected AggregateExpression(string name, IExpressionContext context, [NotNull] Func<IEnumerable<double>, double> aggregate)
            : base(name, context) => _aggregate = aggregate;

        public IEnumerable<IExpression> Values { get; set; } = new List<IExpression>();

        protected override ExpressionResult<double> InvokeCore(IExpressionContext context)
        {
            var values = 
                ExtensionInputOrDefault(ref context, Values)
                    .Enabled()
                    .Select(e => e.Invoke(context))
                    .Values<object>()
                    .Select(v => (double)v);
            var result = _aggregate(values);
            return (result, context);
        }
    }
}