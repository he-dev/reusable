using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;

namespace Reusable
{
    internal class LabelComparer : IComparer<string>
    {
        public int Compare(string left, string right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            {
                return ComparisonResult.Equal;
            }            

            if (ReferenceEquals(left, null))
            {
                return ComparisonResult.LessThen;
            }

            if (ReferenceEquals(right, null))
            {
                return ComparisonResult.GreaterThen;
            }

            var numeric = new[] { left.IsNumeric(), right.IsNumeric() };

            // Identifiers consisting of only digits are compared numerically.
            if (numeric.All(x => x))
            {
                return int.Parse(left).CompareTo(int.Parse(right));
            }

            // Identifiers with letters or hyphens are compared lexically in ASCII sort order.
            if (numeric.All(x => !x))
            {
                return string.Compare(left, right, StringComparison.Ordinal);
            }

            // Numeric identifiers always have lower precedence than non-numeric identifiers.
            return left.IsNumeric() ? ComparisonResult.LessThen : ComparisonResult.GreaterThen;
        }
    }
}