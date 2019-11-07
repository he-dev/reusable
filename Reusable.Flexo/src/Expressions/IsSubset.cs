using System.Collections.Generic;
using System.Linq.Custom;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsSubset : CollectionExtension<bool>
    {
        public IsSubset(ILogger<IsSubset> logger) : base(logger, nameof(IsSubset)) { }

        public IEnumerable<IExpression> Values { get => ThisInner; set => ThisInner = value; }

        [JsonRequired]
        public List<IExpression> Of { get; set; }

        [JsonProperty("Comparer")]
        public string? ComparerName { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            var first = This(context).Enabled().Invoke(context).Values<object>();
            var second = Of.Enabled().Invoke(context).Values<object>();
            var comparer = context.GetEqualityComparerOrDefault(ComparerName);
            return first.IsSubsetOf(second, comparer);
        }
    }
}