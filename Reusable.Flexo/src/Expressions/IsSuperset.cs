using System.Collections.Generic;
using System.Linq.Custom;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    public class IsSuperset : CollectionExtension<bool>
    {
        public IsSuperset(ILogger<IsSuperset> logger) : base(logger, nameof(IsSuperset)) { }

        public IEnumerable<IExpression> Values { get => ThisInner; set => ThisInner = value; }

        [JsonRequired]
        public List<IExpression> Of { get; set; }

        [JsonProperty("Comparer")]
        public string? ComparerName { get; set; }

        protected override bool InvokeAsValue(IImmutableContainer context)
        {
            var first = This(context).Enabled().Invoke(context).Values<object>();
            var second = Of.Invoke(context).Values<object>();
            var comparer = context.GetEqualityComparerOrDefault(ComparerName);
            return first.IsSupersetOf(second, comparer);
        }
    }
}