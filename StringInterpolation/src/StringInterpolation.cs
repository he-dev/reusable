using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable;

public delegate bool TryGetValueFunc(string name, [MaybeNullWhen(false)] out object? value);

[PublicAPI]
public static class StringInterpolation
{
    // https://regex101.com/r/sK1tS8/5
    // language=regexp
    private const string ExpressionPattern = @"(?<!{){(?<Name>[a-zA-Z_][a-zA-Z0-9_.-]*)(,(?<Alignment>-?\d+))?(:(?<FormatString>.*?))?}(?!})";

    private static class Groups
    {
        public const string Name = nameof(Name);
        public const string Alignment = nameof(Alignment);
        public const string FormatString = nameof(FormatString);
    }

    [MustUseReturnValue]
    [ContractAnnotation("text: null => notnull; text: notnull => notnull; tryGetValue: null => stop")]
    public static string Format(this string? text, TryGetValueFunc tryGetValue, IFormatProvider formatProvider)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        var result = Regex.Replace(text, ExpressionPattern, match =>
        {
            var name = match.Groups[Groups.Name].Value;
            var alignment = match.Groups[Groups.Alignment] is { Success: true } a ? $",{a}" : string.Empty;
            var formatString = match.Groups[Groups.FormatString] is { Success: true } f ? $":{f}" : string.Empty;

            return
                tryGetValue(name, out var value)
                    ? string
                        .Format(formatProvider, CreateFormatItem(default, alignment, formatString), value) // format the matched item
                        .Format(tryGetValue, formatProvider) // recursively apply formatting
                    : CreateFormatItem(name, alignment, formatString); // reconstruct the original format item
        }, RegexOptions.Compiled);

        // https://regex101.com/r/zG6tF7/3
        // Format escaped expressions, e.g. "{{over}}" -> "{over}"
        return Regex.Replace(result, "{{(?<contents>.+?)}}", match => $"{{{match.Groups["contents"].Value}}}", RegexOptions.Compiled);
    }

    private static string CreateFormatItem(string? index, string alignment, string formatString) => $"{{{index ?? "0"}{alignment}{formatString}}}";

    [MustUseReturnValue]
    [ContractAnnotation("text: notnull => notnull; text: null => null; tryGetValue: null => halt")]
    public static string Format(this string? text, TryGetValueFunc tryGetValue)
    {
        return text.Format(tryGetValue, CultureInfo.InvariantCulture);
    }

    [MustUseReturnValue]
    [ContractAnnotation("text: notnull => notnull; text: null => null; args: null => halt")]
    public static string Format(this string? text, IDictionary<string, object?> args, IFormatProvider formatProvider)
    {
        return Format(text, args.TryGetValue, formatProvider);
    }

    [MustUseReturnValue]
    [ContractAnnotation("text: notnull => notnull; text: null => null; args: null => halt")]
    public static string Format(this string? text, IDictionary<string, object?> args)
    {
        return Format(text, args.TryGetValue);
    }

    [Pure]
    public static IEnumerable<string> GetNames(string text)
    {
        return
            string.IsNullOrEmpty(text)
                ? Enumerable.Empty<string>()
                : Regex.Matches(text, ExpressionPattern).Select(m => m.Groups[Groups.Name].Value);
    }

    public static IDictionary<string, string> ValidateNames(this IDictionary<string, string> source)
    {
        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed - TopologicalSort will throw if the graph has a cycle.
        var sorted =
            source
                .ToDictionary
                (
                    x => x.Key,
                    x => GetNames(x.Value),
                    StringComparer.OrdinalIgnoreCase
                )
                .Select(x => (x.Key, x.Value))
                .ToDirectedGraph()
                .TopologicalSort(StringComparer.OrdinalIgnoreCase)
                .ToList();

        var missingPlaceholders = sorted.Where(node => !source.ContainsKey(node)).ToList();
        if (missingPlaceholders.Any())
        {
            throw new KeyNotFoundException($"One or more placeholders are missing: [{string.Join(", ", missingPlaceholders)}]");
        }

        return source;
    }
}

public class KeyNotFoundException : Exception
{
    public KeyNotFoundException(string message) : base(message) { }
}