using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Reusable.Data;

namespace Reusable.Flexo
{
    public class Any : CollectionExtension<bool>, IFilter
    {
        public Any() : base(default)
        {
            Matcher = new IsEqual
            {
                Value = Constant.True,
                Matcher = Constant.FromValue("Comparer", "Default")
            };
        }

        public IEnumerable<IExpression> Values
        {
            get => ThisInner;
            set => ThisInner = value;
        }

        [JsonProperty("Predicate")]
        public IExpression Matcher { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            return 
                This(context)
                    .Enabled()
                    .Select(e => e.Invoke(context))
                    .Any(c => this.Equal(context.BeginScopeWithThisOuter(c)));
        }
    }
}