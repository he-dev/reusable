using System;
using System.Collections.Generic;

namespace Reusable.Marbles.Data;

internal class SemanticVersionLabelComparer : IComparer<string>
{
    public static readonly IComparer<string> Default = new SemanticVersionLabelComparer();

    public int Compare(string? left, string? right)
    {
        if (ReferenceEquals(left, right)) return ComparisonResult.Equal;
        if (ReferenceEquals(left, null)) return ComparisonResult.LessThan;
        if (ReferenceEquals(right, null)) return ComparisonResult.GreaterThan;

        var leftIsInteger = int.TryParse(left, out var x);
        var rightIsInteger = int.TryParse(right, out var y);

        // Identifiers consisting of only digits are compared numerically.
        if (leftIsInteger && rightIsInteger) return x.CompareTo(y);

        // Identifiers with letters or hyphens are compared lexically in ASCII sort order.
        if (!leftIsInteger && !rightIsInteger) return string.Compare(left, right, StringComparison.Ordinal);

        // Numeric identifiers always have lower precedence than non-numeric identifiers.
        return leftIsInteger ? ComparisonResult.LessThan : ComparisonResult.GreaterThan;
    }
}