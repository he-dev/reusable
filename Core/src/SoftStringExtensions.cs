using System;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable;

public static class SoftStringExtensions
{
    public static bool IsNullOrEmpty(this SoftString? value) => string.IsNullOrEmpty(value?.ToString());

    public static bool IsNullOrWhiteSpace(this SoftString? value) => string.IsNullOrWhiteSpace(value?.ToString());

    public static bool StartsWith(this SoftString softString, string value)
    {
        return softString.ToString().StartsWith(value.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public static bool EndsWith(this SoftString softString, string value)
    {
        return softString.ToString().EndsWith(value.Trim(), StringComparison.OrdinalIgnoreCase);
    }

    public static bool IsMatch(this SoftString softString, [RegexPattern] string pattern, RegexOptions options = RegexOptions.None)
    {
        return Regex.IsMatch(softString.ToString(), pattern, options | RegexOptions.IgnoreCase);
    }
    
    public static SoftString? ToSoftString(this string? value) => SoftString.Create(value);
}