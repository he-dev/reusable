using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable;
// http://semver.org

[PublicAPI]
public class SemanticVersion : IEquatable<SemanticVersion>, IComparable<SemanticVersion>, IComparer<SemanticVersion>
{
    [RegexPattern]
    public static readonly string Pattern = "^(?<major>0|[1-9]\\d*)\\.(?<minor>0|[1-9]\\d*)\\.(?<patch>0|[1-9]\\d*)(?:-(?<prerelease>(?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\\.(?:0|[1-9]\\d*|\\d*[a-zA-Z-][0-9a-zA-Z-]*))*))";

    //?(?:\\+(?<buildmetadata>[0-9a-zA-Z-]+(?:\\.[0-9a-zA-Z-]+)*))?$";

    public SemanticVersion(int major, int minor, int patch, IEnumerable<string> labels)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Labels = labels.ToList();
    }

    public SemanticVersion(int major, int minor = 0, int patch = 0) : this(major, minor, patch, Enumerable.Empty<string>()) { }

    public static IComparer<SemanticVersion> Comparer { get; } = new SemanticVersionComparer();

    public static SemanticVersion Zero = new(0);

    public int Major { get; }

    public int Minor { get; }

    public int Patch { get; }

    public IEnumerable<string> Labels { get; }

    public bool IsPrerelease => Labels.Any();

    public static SemanticVersion Parse(string value)
    {
        return
            TryParse(value, out var result)
                ? result
                : throw new ArgumentException($"Invalid version format. Expected 'Major.Minor.Patch[-Label]', but found '{value}'.");
    }

    public static bool TryParse(string value, [MaybeNullWhen(false)] out SemanticVersion result)
    {
        result = default;

        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        if (Regex.Match(value.Trim(), Pattern) is { Success: true } match)
        {
            result = new SemanticVersion
            (
                major: int.Parse(match.Groups["major"].Value),
                minor: int.Parse(match.Groups["minor"].Value),
                patch: int.Parse(match.Groups["patch"].Value),
                labels: match.Groups["labels"].Value.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList()
            );
        }

        return result is { };
    }

    public override string ToString()
    {
        var labels = Labels.Any() ? $"-{string.Join(".", Labels)}" : string.Empty;
        return $"{Major}.{Minor}.{Patch}{labels}";
    }

    #region IEquatable

    public override int GetHashCode() => ToString().GetHashCode();

    public override bool Equals(object? obj) => obj is SemanticVersion other && Equals(other);

    public bool Equals(SemanticVersion? obj) => Comparer.Compare(this, obj) == 0;

    #endregion

    #region IComparer

    public int Compare(SemanticVersion? left, SemanticVersion? right) => Comparer.Compare(left, right);

    #endregion

    #region IComparable

    public int CompareTo(SemanticVersion? other) => Compare(this, other);

    #endregion

    public static explicit operator SemanticVersion(string value) => Parse(value);

    public static implicit operator string(SemanticVersion value) => value.ToString();

    #region Operators

    public static bool operator <(SemanticVersion left, SemanticVersion right) => Comparer.Compare(left, right) < 0;

    public static bool operator >(SemanticVersion left, SemanticVersion right) => Comparer.Compare(left, right) > 0;

    public static bool operator ==(SemanticVersion left, SemanticVersion right) => Comparer.Compare(left, right) == 0;

    public static bool operator !=(SemanticVersion left, SemanticVersion right) => !(left == right);

    public static bool operator <=(SemanticVersion left, SemanticVersion right) => Comparer.Compare(left, right) <= 0;

    public static bool operator >=(SemanticVersion left, SemanticVersion right) => Comparer.Compare(left, right) >= 0;

    #endregion
}

internal static class ComparisonResult
{
    public const int LessThan = -1;
    public const int Equal = 0;
    public const int GreaterThan = 1;
}