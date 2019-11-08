using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Overlaps : CollectionExtension<bool>, IFilter
    {
        public Overlaps() : base(default) { }
        
        public IEnumerable<IExpression> First { get => ThisInner; set => ThisInner = value; }
        
        [JsonProperty("With", Required = Required.Always)]
        public List<IExpression> Second { get; set; }

        public IExpression? Matcher { get; set; }
        
        public string? ComparerId { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            var first = InvokeThis(context).Values<object>();
            var second = Second.Invoke(context).Values<object>();
            var comparer = context.GetEqualityComparerOrDefault(ComparerId);
            return first.Intersect(second, comparer).Any();
        }
    }
}