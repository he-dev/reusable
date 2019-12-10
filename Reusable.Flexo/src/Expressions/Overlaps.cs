using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class Overlaps : Extension<object, bool>, IFilter
    {
        public Overlaps() : base(default)
        {
            Matcher = Constant.DefaultComparer;
        }

        public IEnumerable<IExpression>? First
        {
            set => Arg = value;
        }

        [JsonRequired]
        [JsonProperty("With", Required = Required.Always)]
        public List<IExpression> Second { get; set; } = default!;

        [JsonProperty(Filter.Properties.Comparer)]
        public IExpression? Matcher { get; set; }

        protected override bool ComputeSingle(IImmutableContainer context)
        {
            var first = GetArg(context);
            var second = Second.Invoke(context);
            var comparer = this.GetEqualityComparer(context);
            return first.Intersect(second.SelectMany(c => c), comparer).Any();
        }
    }
}