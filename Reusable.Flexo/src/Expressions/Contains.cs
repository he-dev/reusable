using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class Contains : Extension<object, bool>, IFilter
    {
        public Contains() : base(default) { }

        public IEnumerable<IExpression>? Values
        {
            set => Arg = value;
        }

        [JsonProperty(Filter.Properties.Predicate)]
        public IExpression? Matcher { get; set; }

        protected override bool ComputeSingle(IImmutableContainer context)
        {
            return GetArg(context).Any(c => this.Equal(Constant.Single("x", c), context));
        }
    }
}