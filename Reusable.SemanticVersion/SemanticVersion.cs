using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Collections;

namespace Reusable
{
    // http://semver.org

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SemanticVersion : IComparable<SemanticVersion>, IComparer<SemanticVersion>
    {
        public SemanticVersion(int major, int minor, int patch, IEnumerable<string> labels)
        {
            VersionValidator.ValidateMinVersion(major, minor, patch);
            Major = major;
            Minor = minor;
            Patch = patch;
            Labels = (labels ?? throw new ArgumentNullException(nameof(labels))).ToList();
        }

        public SemanticVersion(int major, int minor, int patch) : this(major, minor, patch, Enumerable.Empty<string>()) { }

        public static SemanticVersion Parse(string value)
        {
            if (TryParse(value, out SemanticVersion result)) return result;
            throw new InvalidVersionException($"'{value}' is not a valid version.");
        }

        public static bool TryParse(string value, out SemanticVersion result)
        {
            result = null;

            if (string.IsNullOrEmpty(value)) return false;

            var versionPatterns = new[] { "major", "minor", "patch" }.Select(x => $"(?<{x}>(?!0)[0-9]+|0)");

            var versionMatch = Regex.Match(value.Trim(), $"^v?{string.Join("[\\.]", versionPatterns)}(-(?<labels>[a-z0-9\\.-]+))?$");
            if (versionMatch.Success)
            {
                result = new SemanticVersion
                (
                    major: int.Parse(versionMatch.Groups["major"].Value),
                    minor: int.Parse(versionMatch.Groups["minor"].Value),
                    patch: int.Parse(versionMatch.Groups["patch"].Value),
                    labels: versionMatch.Groups["labels"].Value.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries)
                );
                return true;
            }

            return false;
        }

        private string DebuggerDisplay => ToString();

        public int Major { get; }

        public int Minor { get; }

        public int Patch { get; }

        public IReadOnlyList<string> Labels { get; }

        public bool IsPrerelease => Labels?.Count > 0;

        public override string ToString()
        {
            var labels = Labels.Any() ? $"-{string.Join(".", Labels)}" : string.Empty;
            return $"{Major}.{Minor}.{Patch}{labels}";
        }

        public override bool Equals(object obj)
        {
            return
                !ReferenceEquals(obj, null) &&
                (obj is SemanticVersion other) &&
                other == this;
        }

        public override int GetHashCode() => ToString().GetHashCode();

        #region IComparer

        public int Compare(SemanticVersion left, SemanticVersion right)
        {
            const int less = -1;
            const int equal = 0;
            const int greater = 1;

            if (ReferenceEquals(left, null) && ReferenceEquals(right, null)) return equal;
            if (ReferenceEquals(left, null)) return less;
            if (ReferenceEquals(right, null)) return greater;

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

            if (left.IsPrerelease && !right.IsPrerelease) return less;
            if (!left.IsPrerelease && right.IsPrerelease) return greater;

            // Precedence for two pre-release versions with the same major, minor, and patch version 
            // MUST be determined by comparing each dot separated identifier from left to right 
            // until a difference is found as follows:     

            var labelComparer = new LabelComparer();
            var labelDiffs = left.Labels.ZipWithDefault(right.Labels, (l1, l2) => labelComparer.Compare(l1, l2));
            var firstLabelDiff = labelDiffs.FirstOrDefault(diff => diff != 0);

            return firstLabelDiff;
        }

        #endregion

        #region IComparable

        public int CompareTo(SemanticVersion other) => Compare(this, other);

        #endregion

        public static explicit operator SemanticVersion(string value) => Parse(value);

        public static implicit operator string(SemanticVersion value) => value.ToString();

        public static bool operator <(SemanticVersion left, SemanticVersion right)
        {
            return
                !ReferenceEquals(left, null) &&
                !ReferenceEquals(right, null) &&
                left.CompareTo(right) < 0;
        }

        public static bool operator >(SemanticVersion left, SemanticVersion right)
        {
            return
                !ReferenceEquals(left, null) &&
                !ReferenceEquals(right, null) &&
                left.CompareTo(right) > 0;
        }

        public static bool operator ==(SemanticVersion left, SemanticVersion right)
        {
            return
                !ReferenceEquals(left, null) &&
                !ReferenceEquals(right, null) &&
                left.CompareTo(right) == 0;
        }

        public static bool operator !=(SemanticVersion left, SemanticVersion right)
        {
            return !(left == right);
        }

        public static bool operator <=(SemanticVersion left, SemanticVersion right)
        {
            return
                !ReferenceEquals(left, null) &&
                !ReferenceEquals(right, null) &&
                left.CompareTo(right) <= 0;
        }

        public static bool operator >=(SemanticVersion left, SemanticVersion right)
        {
            return
                !ReferenceEquals(left, null) &&
                !ReferenceEquals(right, null) &&
                left.CompareTo(right) >= 0;
        }
    }
}
