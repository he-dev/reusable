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
        
        public IEnumerable<IExpression> First { get => ThisInner; set => ThisInner = value; }
        
        [JsonProperty("With", Required = Required.Always)]
        public List<IExpression> Second { get; set; }

        [JsonProperty("Comparer")]
        public string? ComparerName { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            var first = This(context).Enabled().Invoke(context).Values<object>();
            var second = Second.Enabled().Invoke(context).Values<object>();
            var comparer = context.GetEqualityComparerOrDefault(ComparerName);
            return first.Intersect(second, comparer).Any();
        }
    }
}