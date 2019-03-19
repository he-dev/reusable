using System.Collections.Generic;
using System.Text.RegularExpressions;
using Reusable.Collections;

namespace Reusable.Flexo
{
    public class SoftStringComparer : EqualityComparerProvider
    {
        public SoftStringComparer(string name) : base(name ?? nameof(SoftStringComparer), ExpressionContext.Empty) { }

        protected override CalculateResult<IEqualityComparer<object>> Calculate(IExpressionContext context)
        {
            var comparer = EqualityComparerFactory<object>.Create
            (
                equals: (left, right) => SoftString.Comparer.Equals((string)left, (string)right),
                getHashCode: (obj) => SoftString.Comparer.GetHashCode((string)obj)
            );
            return (comparer, context);
        }
    }

    public class RegexComparer : EqualityComparerProvider
    {
        public RegexComparer(string name) : base(name ?? nameof(RegexComparer), ExpressionContext.Empty) { }

        public bool IgnoreCase { get; set; }

        protected override CalculateResult<IEqualityComparer<object>> Calculate(IExpressionContext context)
        {
            var options = IgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None;

            var comparer = EqualityComparerFactory<object>.Create
            (
                equals: (left, right) => Regex.IsMatch((string)right, (string)left, options),
                getHashCode: (obj) => 0
            );
            return (comparer, context);
        }
    }
}