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
        public All() : base(default, nameof(All)) { }

        public IEnumerable<IExpression> Values { get => ThisInner; set => ThisInner = value; }

        public IExpression? Predicate { get; set; }

        [JsonProperty("Comparer")]
        public string? ComparerName { get; set; }
        
        protected override bool ComputeValue(IImmutableContainer context)
        {
            return 
                This(context)
                    .Enabled()
                    .Select(item => item.Invoke(context))
                    .All(x => this.Equal(context, x, true));
        }
    }
}