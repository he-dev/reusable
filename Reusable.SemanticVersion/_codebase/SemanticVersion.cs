using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Extensions;
using Reusable.Validations;

namespace Reusable
{
    // http://semver.org

    /// <summary>
    /// Provides functionality for semantic versioning.
    /// </summary>
    [DebuggerDisplay("{ToString(),nq}")]
    public class SemanticVersion : IComparable<SemanticVersion>, IComparer<SemanticVersion>
    {
        private readonly List<string> _labels;

        public SemanticVersion(int major, int minor, int patch, IEnumerable<string> labels = null)
        {
            Major = major.Validate(nameof(major)).IsGreaterThenOrEqual(0).Value;
            Minor = minor.Validate(nameof(minor)).IsGreaterThenOrEqual(0).Value;
            Patch = patch.Validate(nameof(patch)).IsGreaterThenOrEqual(0).Value;
            _labels = labels?.ToList();
        }

        public static SemanticVersion Parse(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }

            var versionPatterns = new[] { "major", "minor", "patch" }.Select(x => $"(?<{x}>(?!0)[0-9]+|0)");

            var versionMatch = Regex.Match(value.Trim(), $"^v?{string.Join("[\\.]", versionPatterns)}(-(?<labels>[a-z0-9\\.-]+))?$");
            if (!versionMatch.Success)
            {
                return null;
            }

            return new SemanticVersion
            (
                major: int.Parse(versionMatch.Groups["major"].Value),
                minor: int.Parse(versionMatch.Groups["minor"].Value),
                patch: int.Parse(versionMatch.Groups["patch"].Value),
                labels: versionMatch.Groups["labels"].Value.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList()
            );
        }

        public int Major { get; }

        public int Minor { get; }

        public int Patch { get; }

        public IReadOnlyList<string> Labels => _labels;

        public bool IsPrerelease => Labels?.Count > 0;

        public override string ToString()
        {
            var versionNumber = $"{Major}.{Minor}.{Patch}";
            if (Labels.Count > 0)
            {
                versionNumber = $"{versionNumber}-{string.Join(".", Labels)}";
            }
            return versionNumber;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null))
            {
                return false;
            }
            var semVer = obj as SemanticVersion;
            if (ReferenceEquals(semVer, null))
            {
                return false;
            }
            return ToString() == semVer.ToString();
        }

        public int Compare(SemanticVersion left, SemanticVersion right)
        {
            const int less = -1;
            const int equal = 0;
            const int greater = 1;

            if (object.Equals(left, null) && object.Equals(right, null))
            {
                return equal;
            }

            if (object.Equals(left, null))
            {
                return less;
            }

            if (object.Equals(right, null))
            {
                return greater;
            }

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
            if (firstVersionDifference != 0)
            {
                return firstVersionDifference;
            }

            // When major, minor, and patch are equal, 
            // a pre-release version has lower precedence than a normal version. 
            // Example: 1.0.0-alpha < 1.0.0.

            if (left.IsPrerelease && !right.IsPrerelease)
            {
                return less;
            }

            if (!left.IsPrerelease && right.IsPrerelease)
            {
                return greater;
            }

            // Precedence for two pre-release versions with the same major, minor, and patch version 
            // MUST be determined by comparing each dot separated identifier from left to right 
            // until a difference is found as follows:     

            var labelComparer = new LabelComparer();
            var labelDiffs = left.Labels.ZipWithDefault(right.Labels, (l1, l2) => labelComparer.Compare(l1, l2));
            var firstLabelDiff = labelDiffs.FirstOrDefault(diff => diff != 0);

            return firstLabelDiff;
        }

        public int CompareTo(SemanticVersion other)
        {
            return Compare(this, other);
        }

        public static explicit operator SemanticVersion(string value)
        {
            return string.IsNullOrEmpty(value) ? null : Parse(value);
        }

        public static implicit operator string(SemanticVersion value)
        {
            return value.ToString();
        }

        public static bool operator <(SemanticVersion left, SemanticVersion right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator >(SemanticVersion left, SemanticVersion right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator ==(SemanticVersion left, SemanticVersion right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
            {
                return false;
            }

            return left.CompareTo(right) == 0;
        }

        public static bool operator !=(SemanticVersion left, SemanticVersion right)
        {
            return !(left == right);
        }

        public static bool operator <=(SemanticVersion left, SemanticVersion right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >=(SemanticVersion left, SemanticVersion right)
        {
            return left.CompareTo(right) >= 0;
        }
    }

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

    internal static class StringExtensions
    {
        public static bool IsNumeric(this string value)
            => !string.IsNullOrEmpty(value) && Regex.IsMatch(value, @"^\d+$");
    }
}
