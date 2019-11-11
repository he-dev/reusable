using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class Where : CollectionExtension<IEnumerable<IExpression>>, IFilter
    {
        public Where() : base(default) { }

        public IEnumerable<IExpression>? Values
        {
            get => Arg;
            set => Arg = value;
        }

        [JsonRequired]
        [JsonProperty(Filter.Properties.Predicate)]
        public IExpression? Matcher { get; set; } = default!;

        protected override IEnumerable<IExpression> ComputeValue(IImmutableContainer context)
        {
            var query =
                from item in GetArg(context).Invoke(context)
                where this.Equal(context, item)
                select item;

            return query.ToList();
        }
    }
}