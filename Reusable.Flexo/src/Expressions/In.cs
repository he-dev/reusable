using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Data;

namespace Reusable.Flexo
{
    [PublicAPI]
    public class In : ScalarExtension<bool>
    {
        public In() : base(default) { }

        public IExpression Value
        {
            get => ThisInner;
            set => ThisInner = value;
        }

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