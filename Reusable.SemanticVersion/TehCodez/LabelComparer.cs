using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Extensions;

namespace Reusable
{
    internal class LabelComparer : IComparer<string>
    {
        public static readonly IComparer<string> Default = new LabelComparer();

        public int Compare(string left, string right)
        {
            if (ReferenceEquals(left, right)) return ComparisonResult.Equal;
            if (ReferenceEquals(left, null)) return ComparisonResult.LessThen;
            if (ReferenceEquals(right, null)) return ComparisonResult.GreaterThen;

            var leftIsInteger = int.TryParse(left, out var x);
            var rightIsInteger = int.TryParse(right, out var y);

            // Identifiers consisting of only digits are compared numerically.
            if (leftIsInteger && rightIsInteger) return x.CompareTo(y);

            // Identifiers with letters or hyphens are compared lexically in ASCII sort order.
            if (!leftIsInteger && !rightIsInteger) return string.Compare(left, right, StringComparison.Ordinal);

            // Numeric identifiers always have lower precedence than non-numeric identifiers.
            return leftIsInteger ? ComparisonResult.LessThen : ComparisonResult.GreaterThen;
        }
    }
}