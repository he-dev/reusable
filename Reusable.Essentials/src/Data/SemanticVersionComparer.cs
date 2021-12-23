using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;

namespace Reusable.Essentials.Data;

public class SemanticVersionComparer : IComparer<SemanticVersion>
{
    public int Compare(SemanticVersion left, SemanticVersion right)
    {
        if (ReferenceEquals(left, right)) return ComparisonResult.Equal;
        if (ReferenceEquals(left, null)) return ComparisonResult.LessThan;
        if (ReferenceEquals(right, null)) return ComparisonResult.GreaterThan;

        // Precedence MUST be calculated by separating the version into:
        // major, minor, patch and pre-release identifiers in that order.
        // (Build metadata does not figure into precedence).

        var xVersions = new[] { left.Major, left.Minor, left.Patch };
        var yVersions = new[] { right.Major, right.Minor, right.Patch };

        // Precedence is determined by the first difference
        // when comparing each of these identifiers from left to right.
        // Example: 1.0.0 < 2.0.0 < 2.1.0 < 2.1.1.
        var versionDifferences = xVersions.Zip(yVersions, (xv, yv) => xv.CompareTo(yv));
        var firstVersionDifference = versionDifferences.FirstOrDefault(diff => diff != 0);
        if (firstVersionDifference != 0) return firstVersionDifference;

        // When major, minor, and patch are equal, 
        // a pre-release version has lower precedence than a normal version. 
        // Example: 1.0.0-alpha < 1.0.0.

        if (left.IsPrerelease && !right.IsPrerelease) return ComparisonResult.LessThan;
        if (!left.IsPrerelease && right.IsPrerelease) return ComparisonResult.GreaterThan;

        // Precedence for two pre-release versions with the same major, minor, and patch version 
        // MUST be determined by comparing each dot separated identifier from left to right 
        // until a difference is found as follows:     

        var labelDiffs = left.Labels.ZipOrDefault(right.Labels, (l1, l2) => SemanticVersionLabelComparer.Default.Compare(l1, l2));
        var firstLabelDiff = labelDiffs.FirstOrDefault(diff => diff != 0);

        return firstLabelDiff;
    }
}