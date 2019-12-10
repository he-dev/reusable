using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class In : Extension<object, bool>, IFilter
    {
        public In() : base(default)
        {
            Matcher = Constant.DefaultComparer;
        }

        public IExpression? Value
        {
            set => Arg = value;
        }

        [JsonRequired]
        public IEnumerable<IExpression> Values { get; set; } = default!;

        [JsonProperty(Filter.Properties.Comparer)]
        public IExpression? Matcher { get; set; }
        
        protected override bool ComputeValue(IImmutableContainer context)
        {
            var x = GetArg(context);
            var y = Values.Enabled().Invoke(context).SelectMany(z => z);
            var c = this.GetEqualityComparer(context);
            return x.Intersect(y, c).Any();
        }

    }
}