using System;
using System.Collections.Generic;

namespace Reusable
{
    internal class LabelComparer : IComparer<string>
    {
        public int Compare(string left, string right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            {
                return 0;
            }

            if (ReferenceEquals(left, null))
            {
                return -1;
            }

            if (ReferenceEquals(right, null))
            {
                return 1;
            }

            // Identifiers consisting of only digits are compared numerically.
            if (left.IsNumeric() && right.IsNumeric())
            {
                return int.Parse(left).CompareTo(int.Parse(right));
            }

            // Identifiers with letters or hyphens are compared lexically in ASCII sort order.
            if (!left.IsNumeric() && !right.IsNumeric())
            {
                return string.Compare(left, right, StringComparison.Ordinal);
            }

            // Numeric identifiers always have lower precedence than non-numeric identifiers.
            return left.IsNumeric() ? -1 : 1;
        }
    }
}