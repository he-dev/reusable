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

        protected AggregateExpression(string name, [NotNull] Func<IEnumerable<double>, double> aggregate)
            : base(name) => _aggregate = aggregate;

        public IEnumerable<IExpression> Values { get; set; } = new List<IExpression>();

        protected override Constant<double> InvokeCore(IExpressionContext context)
        {
            var values =
                ExtensionInputOrDefault(ref context, Values)
                    .Enabled()
                    .Select(e => (double)e.Invoke(context).Value);
                    //.Values<object>()
                    //.Select(v => (double)v);
            var result = _aggregate(values);
            return (Name, result, context);
        }
    }
}