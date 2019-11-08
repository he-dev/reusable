using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class All : CollectionExtension<bool>, IFilter
    {
        public All() : base(default)
        {
            Matcher = new IsEqual
            {
                Value = Constant.True,
                Matcher = Constant.DefaultComparer
            };
        }

        public IEnumerable<IExpression> Values
        {
            get => Arg;
            set => Arg = value;
        }

        [JsonProperty(Filter.Properties.Predicate)]
        public IExpression Matcher { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            return
                GetArg(context)
                    .Select(e => e.Invoke(context))
                    .All(c => this.Equal(context, c));
        }
    }
}