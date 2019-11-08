using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class Contains : CollectionExtension<bool>, IFilter
    {
        public Contains() : base(default) { }

        public IEnumerable<IExpression> Values
        {
            get => Arg;
            set => Arg = value;
        }

        [JsonProperty(Filter.Properties.Predicate)]
        public IExpression? Matcher { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            return
                GetArg(context)
                    .Select(e => e.Invoke(context))
                    .Any(c => this.Equal(context, c));
        }
    }
}