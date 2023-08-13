using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.Extensions;

public static class StringExtensions
{
    [ContractAnnotation("value: null => true; notnull => false")]
    public static bool IsNullOrEmpty([NotNullWhen(false)] this string? value) => string.IsNullOrEmpty(value);

    [ContractAnnotation("value: null => false; notnull => true")]
    public static bool IsNotNullOrEmpty([NotNullWhen(true)] this string? value) => !IsNullOrEmpty(value);

    public static string? NullIfEmpty(this string? value) => string.IsNullOrEmpty(value) ? null : value;

    public static string? NullIfWhitespace(this string? value) => string.IsNullOrWhiteSpace(value) ? null : value;

    public static bool IsInteger(this string value) => int.TryParse(value, out var x);

    public static string QuoteWith(this string? value, string quotationMark)
    {
        return $"{quotationMark}{value}{quotationMark}";
    }

    public static string QuoteWith(this string value, char quotationMark)
    {
        return value.QuoteWith(quotationMark.ToString());
    }

    public static string EncloseWith(this string value, string left, string right, int padding = 0)
    {
        return $"{left}{new string(' ', padding)}{value}{new string(' ', padding)}{right}";
    }

    public static string EncloseWith(this string value, char left, char right, int padding = 0)
    {
        return value.EncloseWith(left.ToString(), right.ToString(), padding);
    }

    public static string EncloseWith(this string value, string leftRight, int padding = 0)
    {
        if (leftRight.Length != 2) throw new ArgumentOutOfRangeException($"{nameof(leftRight)} must contain exactly two characters.");
        return value.EncloseWith(leftRight[0], leftRight[1], padding);
    }

    public static bool Contains(this string value, string other, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
    {
        return value.IndexOf(other, comparisonType) >= 0;
    }

    [ContractAnnotation("value: null => halt; encoding: null => halt")]
    public static StreamReader ToStreamReader(this string value, Encoding encoding)
    {
        return new StreamReader(new MemoryStream(encoding.GetBytes(value)));
    }

    [ContractAnnotation("value: null => halt; value: notnull => notnull")]
    public static Stream ToMemoryStream(this string value, Encoding? encoding = default)
    {
        return new MemoryStream((encoding ?? Encoding.UTF8).GetBytes(value));
    }

    public static bool Matches(this string? value, [RegexPattern] string pattern, RegexOptions options = RegexOptions.None)
    {
        return value is { } && Regex.IsMatch(value, pattern, options);
    }

    public static bool Matches(this string? value, [RegexPattern] IEnumerable<string> patterns, RegexOptions options = RegexOptions.None)
    {
        return value is { } && patterns.Any(pattern => Regex.IsMatch(value, pattern, options));
    }

    public static string Capitalize(this string? value) => value is { } ? Regex.Replace(value, @"\A([a-z]+)", m => m.Value.ToUpper()) : string.Empty;

    //[ContractAnnotation("value:null => null; value:notnull => notnull")]
    [return: NotNullIfNotNull(nameof(value))]
    public static string? RegexReplace(this string? value, [RegexPattern] string pattern, string replacement = "", RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Compiled)
    {
        return value is not null ? Regex.Replace(value, pattern, replacement, options) : default;
    }

    public static string IndentLines(this string value, int indentWidth, char indentCharacter = ' ', Encoding? encoding = default)
    {
        if (indentWidth < 0) throw new ArgumentOutOfRangeException(nameof(indentWidth));

        var output = new StringBuilder();
        using (var valueStream = new MemoryStream((encoding ?? Encoding.UTF8).GetBytes(value)))
        using (var valueReader = new StreamReader(valueStream))
        {
            while (valueReader.ReadLine() is var line && line != null)
            {
                output
                    .Append(new string(indentCharacter, indentWidth))
                    .AppendLine(line);
            }
        }

        return
            output
                .TrimEnd(Environment.NewLine)
                .ToString();
    }

    /// <summary>
    /// Matches the specified string by glob-pattern.
    /// </summary>
    public static bool WildcardMatches(this string path, string pattern)
    {
        pattern = pattern.RegexReplace(@"\\", @"\\");
        pattern = pattern.RegexReplace(@"\.", @"\.");
        pattern = pattern.RegexReplace(@"\?", @".");
        pattern = pattern.RegexReplace(@"\*", @".*?");

        return Regex.IsMatch(path, $"^{pattern}$", RegexOptions.IgnoreCase);
    }

    /// <summary>
    /// Removes all spaces from a string.
    /// </summary>
    [ContractAnnotation("value: null => null; notnull => notnull")]
    public static string? Minify(this string? value)
    {
        return value is { } ? Regex.Replace(value, @"\s", string.Empty) : default;
    }
}