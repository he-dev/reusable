using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class Where : Extension<object, object>, IFilter
    {
        public Where() : base(default) { }

        public IEnumerable<IExpression>? Values
        {
            set => Arg = value;
        }

        [JsonRequired]
        [JsonProperty(Filter.Properties.Predicate)]
        public IExpression? Matcher { get; set; } = default!;

        protected override IEnumerable<object> ComputeMany(IImmutableContainer context)
        {
            return 
                from item in GetArg(context)
                where this.Equal(Constant.Single($"{nameof(Where)}.Item", item), context)
                select item;
        }
    }
}