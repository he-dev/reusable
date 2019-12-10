using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class IsSubset : Extension<object, bool>, IFilter
    {
        public IsSubset() : base(default) => Matcher = Constant.DefaultComparer;

        public IEnumerable<IExpression>? Values
        {
            set => Arg = value;
        }

        [JsonRequired]
        public List<IExpression> Of { get; set; } = default!;

        [JsonProperty(Filter.Properties.Comparer)]
        public IExpression? Matcher { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            var first = GetArg(context);
            var second = Of.Enabled().Invoke(context).SelectMany(c => c);
            var comparer = this.GetEqualityComparer(context);
            return first.IsSubsetOf(second, comparer);
        }
    }
}