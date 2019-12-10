using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class All : Extension<object, bool>, IFilter
    {
        public All() : base(default)
        {
            Matcher = new IsEqual
            {
                Value = Constant.True,
                Matcher = Constant.DefaultComparer
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
            return GetArg(context).All(c => this.Equal(Constant.Single($"{nameof(All)}.Item", c), context));
        }
    }
}