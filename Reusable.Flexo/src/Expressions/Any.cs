using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Data;

namespace Reusable.Flexo
{
    public class Any : CollectionExtension<bool>, IFilter
    {
        public Any() : base(default, nameof(Any)) { }

        public IEnumerable<IExpression> Values
        {
            get => ThisInner;
            set => ThisInner = value;
        }

        public IExpression? Predicate { get; set; }

        [JsonProperty("Comparer")]
        public string? ComparerName { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            return 
                This(context)
                    .Enabled()
                    .Select(item => item.Invoke(context))
                    .Any(x => this.Equal(context, x, true));
        }
    }
}