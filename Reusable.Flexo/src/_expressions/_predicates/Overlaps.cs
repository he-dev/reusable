using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Overlaps : CollectionExtension<bool>
    {
        public Overlaps(ILogger<Overlaps> logger) : base(logger, nameof(Overlaps)) { }

        [JsonProperty("Values")]
        public override IEnumerable<IExpression> This { get; set; }

        public List<IExpression> With { get; set; } = new List<IExpression>();

        public string Comparer { get; set; }

        protected override Constant<bool> InvokeCore(IImmutableSession context, IEnumerable<IExpression> @this)
        {
            var with = With.Invoke(context).Values<object>();
            var comparer = context.GetComparerOrDefault(Comparer);
            var values = @this.Invoke(context).Values<object>();
            return (Name, values.Intersect(with, comparer).Any(), context);
        }
    }
}