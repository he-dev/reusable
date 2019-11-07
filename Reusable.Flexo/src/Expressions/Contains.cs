using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.OmniLog.Abstractions;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class Contains : CollectionExtension<bool>
    {
        public Contains() : base(default, nameof(Contains)) { }

        public IEnumerable<IExpression> Values { get => ThisInner; set => ThisInner = value; }

        public IExpression Value { get; set; }

        [JsonProperty("Comparer")]
        public string? ComparerName { get; set; }

        protected override bool ComputeValue(IImmutableContainer context)
        {
            var value = Value.Invoke(context).Value;
            var comparer = context.GetEqualityComparerOrDefault(ComparerName);
            return 
                This(context)
                    .Enabled()
                    .Any(x => comparer.Equals(value, x.Invoke(context).Value<object>()));
        }
    }

    [PublicAPI]
    public class In : ScalarExtension<bool>
    {
        public In() : base(default, nameof(In)) { }

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