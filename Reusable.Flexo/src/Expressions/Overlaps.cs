using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class Overlaps : CollectionExtension<bool>, IFilter
    {
        public Overlaps() : base(default)
        {
            Matcher = Constant.DefaultComparer;
        }
        
        public IEnumerable<IExpression> First { get => Arg; set => Arg = value; }
        
        [JsonProperty("With", Required = Required.Always)]
        public List<IExpression> Second { get; set; }

        [JsonProperty(Filter.Properties.Comparer)]
        public IExpression Matcher { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            var first = InvokeArg(context).Values<object>();
            var second = Second.Invoke(context).Values<object>();
            var comparer = this.GetEqualityComparer(context);
            return first.Intersect(second, comparer).Any();
        }
    }
}