using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable;

public class SemanticVersionComparer : IComparer<SemanticVersion>
{
    public static readonly IComparer<SemanticVersion> Default = new SemanticVersionComparer();

    public int Compare(SemanticVersion? left, SemanticVersion? right)
    {
        if (ReferenceEquals(left, right)) return ComparisonResult.Equal;
        if (ReferenceEquals(left, null)) return ComparisonResult.LessThan;
        if (ReferenceEquals(right, null)) return ComparisonResult.GreaterThan;

        // Precedence MUST be calculated by separating the version into:
        // major, minor, patch and pre-release identifiers in that order.
        // (Build metadata does not figure into precedence).

        var versionsL = new[] { left.Major, left.Minor, left.Patch };
        var versionsR = new[] { right.Major, right.Minor, right.Patch };

        // Precedence is determined by the first difference
        // when comparing each of these identifiers from left to right.
        // Example: 1.0.0 < 2.0.0 < 2.1.0 < 2.1.1.
        var versionDifferences = versionsL.Zip(versionsR, (l, r) => l.CompareTo(r));
        var firstVersionDifference = versionDifferences.FirstOrDefault(diff => diff != ComparisonResult.Equal);
        if (firstVersionDifference != ComparisonResult.Equal) return firstVersionDifference;

        // When major, minor, and patch are equal, 
        // a pre-release version has lower precedence than a normal version. 
        // Example: 1.0.0-alpha < 1.0.0.

        if (left.IsPrerelease && !right.IsPrerelease) return ComparisonResult.LessThan;
        if (!left.IsPrerelease && right.IsPrerelease) return ComparisonResult.GreaterThan;

        // Precedence for two pre-release versions with the same major, minor, and patch version 
        // MUST be determined by comparing each dot separated identifier from left to right 
        // until a difference is found as follows:     

        var labelDiffs = left.Labels.Zip(right.Labels, (l1, l2) => SemanticVersionLabelComparer.Default.Compare(l1, l2));
        var firstLabelDiff = labelDiffs.FirstOrDefault(diff => diff != ComparisonResult.Equal);

        return firstLabelDiff;
    }
}


/// <summary>
/// Compares a single pair of labels.
/// </summary>
public class SemanticVersionLabelComparer : IComparer<string>
{
    public static readonly IComparer<string> Default = new SemanticVersionLabelComparer();

    public int Compare(string? left, string? right)
    {
        if (ReferenceEquals(left, right)) return ComparisonResult.Equal;
        if (ReferenceEquals(left, default)) return ComparisonResult.LessThan;
        if (ReferenceEquals(right, default)) return ComparisonResult.GreaterThan;

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