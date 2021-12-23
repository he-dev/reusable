﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Essentials.Diagnostics;
using Reusable.Essentials.Extensions;

namespace Reusable.Essentials.Data;
// http://semver.org

[PublicAPI]
[DebuggerDisplay(DebuggerDisplayString.DefaultNoQuotes)]
public class SemanticVersion : IEquatable<SemanticVersion>, IComparable<SemanticVersion>, IComparer<SemanticVersion>
{
    // private static readonly ValidationRuleCollection<SemanticVersion, object> VersionRules = 
    //     ValidationRuleCollection
    //         .For<SemanticVersion>()
    //         .Reject(b => b.When(x => x.Major < 0))
    //         .Reject(b => b.When(x => x.Minor < 0))
    //         .Reject(b => b.When(x => x.Patch < 0));


    private static readonly string Pattern;

    static SemanticVersion()
    {
        var versionPatterns = new[] { "major", "minor", "patch" }.Select(x => $"(?<{x}>(?!0)[0-9]+|0)");
        Pattern = $"^(?<prefix>v)?{string.Join("[\\.]", versionPatterns)}(-(?<labels>[a-z0-9\\.-]+))?$";
    }

    public SemanticVersion(int major, int minor, int patch, IEnumerable<string> labels)
    {
        Major = major;
        Minor = minor;
        Patch = patch;
        Labels = labels.ToImmutableList();
        //this.ValidateWith(VersionRules).ThrowOnFailure();
    }

    public SemanticVersion(int major, int minor, int patch)
        : this(major, minor, patch, Enumerable.Empty<string>()) { }

    public static IComparer<SemanticVersion> Comparer { get; } = new SemanticVersionComparer();

    public static SemanticVersion Zero = new(0, 0, 0);

    public bool Prefix { get; set; } = true;

    public int Major { get; }

    public int Minor { get; }

    public int Patch { get; }

    [NotNull, ItemNotNull]
    public IImmutableList<string> Labels { get; }

    public bool IsPrerelease => Labels.Any();

    private string DebuggerDisplay => ToString();

    [ContractAnnotation("value: null => halt")]
    public static SemanticVersion Parse(string value)
    {
        if (TryParse(value, out var result))
        {
            return result;
        }

        throw ("VersionFormatException", $"Expected version format is 'Major.Minor.Patch[-Label]', but found {value.QuoteWith("'")}.").ToDynamicException();
    }

    [ContractAnnotation("value: null => false, result: null")]
    public static bool TryParse(string value, out SemanticVersion result)
    {
        result = default;
            
        if (string.IsNullOrEmpty(value))
        {
            return false;
        }

        var versionMatch = Regex.Match(value.Trim(), Pattern);
        if (versionMatch.Success)
        {
            result = new SemanticVersion
            (
                major: int.Parse(versionMatch.Groups["major"].Value),
                minor: int.Parse(versionMatch.Groups["minor"].Value),
                patch: int.Parse(versionMatch.Groups["patch"].Value),
                labels: versionMatch.Groups["labels"].Value.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries).ToList()
            )
            {
                Prefix = versionMatch.Groups["prefix"].Success
            };
        }

        return result is not null;
    }

    public override string ToString()
    {
        var prefix = Prefix ? "v" : string.Empty;
        var labels = Labels.Any() ? $"-{string.Join(".", Labels)}" : string.Empty;
        return $"{prefix}{Major}.{Minor}.{Patch}{labels}";
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