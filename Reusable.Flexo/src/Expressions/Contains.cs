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

        public IEnumerable<IExpression> Values { get => ThisInner; set => ThisInner = value; }

        [JsonProperty("Value")]
        public IExpression? Matcher { get; set; }

        public string? ComparerId { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            return 
                This(context)
                    .Enabled()
                    .Select(x => x.Invoke(context))
                    .Any(x => this.Equal(context, x, default));
        }
    }

    [PublicAPI]
    public class In : ScalarExtension<bool>
    {
        public In() : base(default) { }

        public IExpression Value { get => ThisInner; set => ThisInner = value; }

        public IEnumerable<IExpression> Values { get; set; }

        public string Comparer { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            var value = This(context).Invoke(context).Value;
            var comparer = context.GetEqualityComparerOrDefault(Comparer);
            return Values.Enabled().Any(x => comparer.Equals(value, x.Invoke(context).Value));
        }
    }
}