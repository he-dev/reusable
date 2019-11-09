using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Flexo.Abstractions;

namespace Reusable.Flexo
{
    public class Any : CollectionExtension<bool>, IFilter
    {
        public Any() : base(default)
        {
            Matcher = new IsEqual
            {
                Value = Constant.True
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
                    .Any(c => this.Equal(context, c));
        }
    }
}