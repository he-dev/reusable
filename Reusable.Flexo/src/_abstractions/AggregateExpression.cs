using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public abstract class AggregateExpression : CollectionExtension<double>
    {
        private readonly Func<IEnumerable<double>, double> _aggregate;

        protected AggregateExpression(ILogger logger, string name, [NotNull] Func<IEnumerable<double>, double> aggregate)
            : base(logger, name) => _aggregate = aggregate;

        [JsonProperty("Values")]
        public override IEnumerable<IExpression> This { get; set; }

        protected override Constant<double> InvokeCore(IImmutableSession context, IEnumerable<IExpression> @this)
        {
            var values = @this.Select(e => e.Invoke(context)).Values<object>().Cast<double>();
            var result = _aggregate(values);
            return (Name, result, context);
        }
    }
}