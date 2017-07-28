using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable
{
    // http://semver.org

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class SemanticVersion : IComparable<SemanticVersion>, IComparer<SemanticVersion>
    {
        private int _major;
        private int _minor;
        private int _patch;

        [NotNull]
        [ItemNotNull]
        private List<string> _labels = new List<string>();

        private string DebuggerDisplay => ToString();

        public int Major
        {
            get => _major;
            set => _major = ValidateVersion(value);
        }

        public int Minor
        {
            get => _minor;
            set => _minor = ValidateVersion(value);
        }

        public int Patch
        {
            get => _patch;
            set => _patch = ValidateVersion(value);
        }

        private static int ValidateVersion(int version, [CallerMemberName] string memberName = null)
        {
            if (version < 0) throw new ArgumentOutOfRangeException($"{memberName} version must be >= 0.");
            return version;
        }

        [NotNull, ItemNotNull]
        public List<string> Labels
        {
            get => _labels;
            set => _labels = value ?? throw new ArgumentNullException(nameof(Labels));
        }

        public bool IsPrerelease => Labels.Any();

        [ContractAnnotation("value: null => halt")]
        public static SemanticVersion Parse(string value)
        {
            if (TryParse(value, out SemanticVersion result))
            {
                return result;
            }
            throw new InvalidVersionException($"'{value}' is not a valid version.");
        }

        [ContractAnnotation("value: null => false, result: null")]
        public static bool TryParse(string value, out SemanticVersion result)
        {
            if (string.IsNullOrEmpty(value))
            {
                result = default(SemanticVersion);
                return false;
            }

            var versionPatterns = new[] { "major", "minor", "patch" }.Select(x => $"(?<{x}>(?!0)[0-9]+|0)");

            var versionMatch = Regex.Match(value.Trim(), $"^v?{string.Join("[\\.]", versionPatterns)}(-(?<labels>[a-z0-9\\.-]+))?$");
            if (versionMatch.Success)
            {
                result = new SemanticVersion
                {
                    Major = int.Parse(versionMatch.Groups["major"].Value),
                    Minor = int.Parse(versionMatch.Groups["minor"].Value),
                    Patch = int.Parse(versionMatch.Groups["patch"].Value),
                    Labels = versionMatch.Groups["labels"].Value.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList()
                };
                return true;
            }

            result = default(SemanticVersion);
            return false;
        }

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
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null)) return ComparisonResult.Equal;
            if (ReferenceEquals(left, null)) return ComparisonResult.LessThen;
            if (ReferenceEquals(right, null)) return ComparisonResult.GreaterThen;

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

            if (left.IsPrerelease && !right.IsPrerelease) return ComparisonResult.LessThen;
            if (!left.IsPrerelease && right.IsPrerelease) return ComparisonResult.GreaterThen;

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

        public static implicit operator string(SemanticVersion value) => value?.ToString();

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
