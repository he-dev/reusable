using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Extensions;
using Reusable.FluentValidation;
using Reusable.FluentValidation.Validations;

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
        public static SemanticVersion Parse(string version)
        {
            version.Validate(nameof(version)).IsNotNullOrEmpty();

            var versionMatch = Regex.Match(version, @"v?(?<major>\d+)\.(?<minor>\d+)\.(?<patch>\d+)(-(?<labels>.+))?", RegexOptions.IgnoreCase);
            versionMatch.Success.Validate(nameof(version)).IsTrue("Invalid version format. Expected version: 1.2.3[-label]");

            var semVer = new SemanticVersion
            {
                Major = int.Parse(versionMatch.Groups["major"].Value),
                Minor = int.Parse(versionMatch.Groups["minor"].Value),
                Patch = int.Parse(versionMatch.Groups["patch"].Value),
                Labels = versionMatch.Groups["labels"].Value.Split(new[] { '.', '-' }, StringSplitOptions.RemoveEmptyEntries).ToList()
            };
            return semVer;
        }

        public int Major { get; private set; }

        public int Minor { get; private set; }

        public int Patch { get; private set; }

        public List<string> Labels { get; private set; }

        public bool IsPreRelease => Labels?.Count > 0;

        public override string ToString()
        {
            var versionNumber = $"{Major}.{Minor}.{Patch}";
            if (Labels.Count > 0)
            {
                versionNumber = $"{versionNumber}-${string.Join(".", Labels)}";
            }
            return versionNumber;
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, null)) { return false; }
            var semVer = obj as SemanticVersion;
            if (ReferenceEquals(semVer, null)) { return false; }
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

            var preReleases = new[] { x.IsPreRelease, y.IsPreRelease };
            if (preReleases.Any(i => i) && !preReleases.All(i => i))
            {
                if (x.IsPreRelease) { return less; }
                if (y.IsPreRelease) { return greater; }
            }

            // compare versions

            var xVersions = new[] { x.Major, x.Minor, x.Patch };
            var yVersions = new[] { y.Major, y.Minor, y.Patch };

            var diffs = xVersions.Zip(yVersions, (v1, v2) => v1.CompareTo(v2));
            var firstDiff = diffs.FirstOrDefault(diff => diff != 0);

            if (firstDiff != 0)
            {
                return firstDiff;
            }

            var compareLabels = new Func<string, string, int>((l1, l2) =>
            {
                var ints = new int[2];
                if (int.TryParse(l1, out ints[0]) && int.TryParse(l2, out ints[1]))
                {
                    return ints[0].CompareTo(ints[1]);
                }

                return Math.Sign(string.Compare(l1, l2, StringComparison.Ordinal));
            });

            var labelDiffs = x.Labels.ZipWithDefault(y.Labels, (l1, l2) => compareLabels(l1, l2));
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
}
