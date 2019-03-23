using System.Collections.Generic;
using Reusable.Collections;

namespace Reusable.Flexo
{
    public class SoftStringComparer : EqualityComparerProvider
    {
        public SoftStringComparer(string name) : base(name ?? nameof(SoftStringComparer), ExpressionContext.Empty) { }

        protected override ExpressionResult<IEqualityComparer<object>> InvokeCore(IExpressionContext context)
        {
            var comparer = EqualityComparerFactory<object>.Create
            (
                equals: (left, right) => SoftString.Comparer.Equals((string)left, (string)right),
                getHashCode: (obj) => SoftString.Comparer.GetHashCode((string)obj)
            );
            return (comparer, context);
        }
    }
}