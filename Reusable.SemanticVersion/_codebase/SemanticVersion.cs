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
        private SemanticVersion() { }

        public SemanticVersion(int major, int minor, int patch, IEnumerable<string> labels = null)
        {
            Minor = major.Validate(nameof(major)).IsGreaterThenOrEqual(0).Value;
            Minor = minor.Validate(nameof(minor)).IsGreaterThenOrEqual(0).Value;
            Patch = patch.Validate(nameof(patch)).IsGreaterThenOrEqual(0).Value;
            Labels = labels?.ToList();
        }

        public static SemanticVersion Parse(string value)
        {
            value.Validate(nameof(value)).IsNotNullOrEmpty();

            var versionMatch = Regex.Match(value, @"v?(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(-(?<labels>.+))?", RegexOptions.IgnoreCase);
            if (!versionMatch.Success)
            {
                return null;
            }

            return new SemanticVersion
            {
                Major = int.Parse(versionMatch.Groups["major"].Value),
                Minor = int.Parse(versionMatch.Groups["minor"].Value),
                Patch = int.Parse(versionMatch.Groups["patch"].Value),
                Labels = versionMatch.Groups["labels"].Value.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList()
            };
        }

        public int Major { get; private set; }

        public int Minor { get; private set; }

        public int Patch { get; private set; }

        public List<string> Labels { get; private set; }

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

        public int Compare(SemanticVersion x, SemanticVersion y)
        {
            const int less = -1;
            const int equal = 0;
            const int greater = 1;

            if (object.Equals(x, null) && object.Equals(y, null))
            {
                return equal;
            }

            if (object.Equals(x, null))
            {
                return less;
            }

            if (object.Equals(y, null))
            {
                return greater;
            }

            // Precedence MUST be calculated by separating the version into:
            // major, minor, patch and pre-release identifiers in that order.
            // (Build metadata does not figure into precedence).

            var xVersions = new[] { x.Major, x.Minor, x.Patch };
            var yVersions = new[] { y.Major, y.Minor, y.Patch };

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

            if (x.IsPrerelease && !y.IsPrerelease)
            {
                return -1;
            }

            if (!x.IsPrerelease && y.IsPrerelease)
            {
                return 1;
            }

            // Precedence for two pre-release versions with the same major, minor, and patch version 
            // MUST be determined by comparing each dot separated identifier from left to right 
            // until a difference is found as follows:     

            var labelComparer = new LabelComparer();
            var labelDiffs = x.Labels.ZipWithDefault(y.Labels, (l1, l2) => labelComparer.Compare(l1, l2));
            var firstLabelDiff = labelDiffs.FirstOrDefault(diff => diff != 0);

            return firstLabelDiff;
        }

        public int CompareTo(SemanticVersion other)
        {
            return Compare(this, other);
        }

        public static explicit operator SemanticVersion(string semVer)
        {
            return string.IsNullOrEmpty(semVer) ? null : Parse(semVer);
        }

        public static implicit operator string(SemanticVersion semVer)
        {
            return semVer.ToString();
        }

        public static bool operator <(SemanticVersion semVer1, SemanticVersion semVer2)
        {
            return semVer1.CompareTo(semVer2) < 0;
        }

        public static bool operator >(SemanticVersion semVer1, SemanticVersion semVer2)
        {
            return semVer1.CompareTo(semVer2) > 0;
        }

        public static bool operator ==(SemanticVersion semVer1, SemanticVersion semVer2)
        {
            if (ReferenceEquals(semVer1, semVer2))
            {
                return true;
            }

            if (ReferenceEquals(semVer1, null) || ReferenceEquals(semVer2, null))
            {
                return false;
            }

            return semVer1.CompareTo(semVer2) == 0;
        }

        public static bool operator !=(SemanticVersion semVer1, SemanticVersion semVer2)
        {
            return !(semVer1 == semVer2);
        }

        public static bool operator <=(SemanticVersion semVer1, SemanticVersion semVer2)
        {
            return semVer1 < semVer2 || semVer1 == semVer2;
        }

        public static bool operator >=(SemanticVersion semVer1, SemanticVersion semVer2)
        {
            return semVer1 > semVer2 || semVer1 == semVer2;
        }
    }

    internal class LabelComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (ReferenceEquals(x, null) && ReferenceEquals(y, null))
            {
                return 0;
            }

            if (ReferenceEquals(x, null))
            {
                return -1;
            }

            if (ReferenceEquals(y, null))
            {
                return 1;
            }

            // Identifiers consisting of only digits are compared numerically.
            if (x.IsNumeric() && y.IsNumeric())
            {
                return int.Parse(x).CompareTo(int.Parse(y));
            }

            // Identifiers with letters or hyphens are compared lexically in ASCII sort order.
            if (!x.IsNumeric() && !y.IsNumeric())
            {
                return string.Compare(x, y, StringComparison.Ordinal);
            }

            // Numeric identifiers always have lower precedence than non-numeric identifiers.
            return x.IsNumeric() ? -1 : 1;
        }
    }

    internal static class StringExtensions
    {
        public static bool IsNumeric(this string value) 
            => !string.IsNullOrEmpty(value) && Regex.IsMatch(value, @"^\d+$");
    }
}
