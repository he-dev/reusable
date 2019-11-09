using System.Collections.Generic;
using System.Linq.Custom;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsSubset : CollectionExtension<bool>, IFilter
    {
        public IsSubset() : base(default)
        {
            Matcher = Constant.DefaultComparer;
        }

        public IEnumerable<IExpression> Values { get => Arg; set => Arg = value; }

        [JsonRequired]
        public List<IExpression> Of { get; set; }

        
        [JsonProperty(Filter.Properties.Comparer)]
        public IExpression Matcher { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            var first = GetArg(context).Invoke(context).Values<object>();
            var second = Of.Enabled().Invoke(context).Values<object>();
            var comparer = this.GetEqualityComparer(context);
            return first.IsSubsetOf(second, comparer);
        }

    }
}