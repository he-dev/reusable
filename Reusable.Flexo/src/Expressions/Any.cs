using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class Any : Extension<object, bool>, IFilter
    {
        public Any() : base(default)
        {
            Matcher = new IsEqual
            {
                Value = Constant.True
            };
        }

        public IEnumerable<IExpression>? Values
        {
            set => Arg = value;
        }

        [JsonProperty(Filter.Properties.Predicate)]
        public IExpression? Matcher { get; set; }

        protected override bool ComputeSingle(IImmutableContainer context)
        {
            return GetArg(context).Any(c => this.Equal(Constant.Single($"{nameof(Any)}.Item", c), context));
        }
    }
}