using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.Marbles.Extensions;

public delegate bool TryGetValueCallback(string name, [MaybeNullWhen(true)] out object value);

public interface ITryGetValue<in TKey, TValue>
{
    bool TryGetValue(TKey key, [MaybeNullWhen(true)] out TValue value);
}

[PublicAPI]
public static class StringInterpolation
{
    // https://regex101.com/r/sK1tS8/5
    // language=regexp
    private const string ExpressionPattern = "(?<!{){(?<Name>[a-zA-Z_][a-zA-Z0-9_.-]*)(,(?<Alignment>-?\\d+))?(:(?<FormatString>.*?))?}(?!})";

    private static class Groups
    {
        public const string Name = nameof(Name);
        public const string Alignment = nameof(Alignment);
        public const string FormatString = nameof(FormatString);
    }

    [MustUseReturnValue]
    [ContractAnnotation("text: null => null; text: notnull => notnull; tryGetValue: null => stop")]
    public static string Format(this string? text, TryGetValueCallback tryGetValue, IFormatProvider formatProvider)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;

        var result = Regex.Replace(text, ExpressionPattern, match =>
        {
            var name = match.Group(Groups.Name);
            var alignment = match.Group(Groups.Alignment, x => $",{x}");
            var formatString = match.Group(Groups.FormatString, x => $":{x}");

            return
                tryGetValue(name, out var value)
                    // Recursively apply formatting.
                    ? string
                        .Format(formatProvider, CreateCompositeFormatString(), value)
                        .Format(tryGetValue, formatProvider)
                    // Reconstruct the composite format string.
                    : CreateCompositeFormatString(name);

            string CreateCompositeFormatString(string nameOrDefault = "0") => $"{{{nameOrDefault}{alignment}{formatString}}}";
        }, RegexOptions.Compiled);

        // https://regex101.com/r/zG6tF7/3
        // Format escaped expressions, e.g. "{{over}}" -> "{over}"
        return Regex.Replace(result, "{{(?<contents>.+?)}}", match => $"{{{match.Groups["contents"].Value}}}", RegexOptions.Compiled);
    }

    [MustUseReturnValue]
    [ContractAnnotation("text: null => notnull; text: notnull => notnull")]
    public static string Format(this string? text, ITryGetValue<string, object> tryGetValue, IFormatProvider? formatProvider = default)
    {
        return text.Format((string name, out object value) => tryGetValue.TryGetValue(name, out value), formatProvider);
    }

    [MustUseReturnValue]
    [ContractAnnotation("text: notnull => notnull; text: null => null; tryGetValue: null => halt")]
    public static string Format(this string text, TryGetValueCallback tryGetValue)
    {
        return text.Format(tryGetValue, CultureInfo.InvariantCulture);
    }

    [MustUseReturnValue]
    [ContractAnnotation("text: notnull => notnull; text: null => null; args: null => halt")]
    public static string Format(this string? text, IDictionary<string, object> args, IFormatProvider formatProvider)
    {
        return Format(text, args.ValidateNames().TryGetValue, formatProvider);
    }

    [MustUseReturnValue]
    [ContractAnnotation("text: notnull => notnull; text: null => null; args: null => halt")]
    public static string Format(this string? text, IDictionary<string, object> args)
    {
        return Format(text, args.ValidateNames().TryGetValue);
    }

    [MustUseReturnValue]
    [ContractAnnotation("text: notnull => notnull; text: null => null; args: null => halt")]
    public static string Format(this string? text, IDictionary<SoftString, object> args)
    {
        return Format(text, (string name, out object value) => args.TryGetValue(name, out value));
    }

    [MustUseReturnValue]
    [ContractAnnotation("text: null => null; args: null => stop")]
    public static string Format(this string? text, object args, IEqualityComparer<string> comparer, IFormatProvider formatProvider)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var properties =
            args
                .GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                //.Where(p => p.IsDefined(typeof(IgnoreAttribute))) // todo - add ignore attribute
                .ToDictionary(p => p.Name, p => p, comparer);

        return Format(text, (string name, out object value) =>
        {
            if (properties.TryGetValue(name, out var property) && property.GetValue(args) is { } result)
            {
                value = result;
                return true;
            }
            else
            {
                value = default!;
                return false;
            }
        }, formatProvider);
    }

    [ContractAnnotation("text: null => notnull; args: null => stop")]
    public static string Format(this string text, object args)
    {
        return text.Format(args, StringComparer.OrdinalIgnoreCase, CultureInfo.InvariantCulture) ?? string.Empty;
    }

    [Pure]
    public static IEnumerable<string> GetNames(string text)
    {
        return
            string.IsNullOrEmpty(text)
                ? Enumerable.Empty<string>()
                : Regex.Matches(text, ExpressionPattern).Select(m => m.Groups[Groups.Name].Value);
    }

    private static IDictionary<string, object> ValidateNames(this IDictionary<string, object> replacements)
    {
        var placeholders =
            replacements
                .Where(x => x.Value is string)
                .ToDictionary
                (
                    x => x.Key,
                    x => GetNames((string)x.Value),
                    StringComparer.OrdinalIgnoreCase
                );

        // ReSharper disable once ReturnValueOfPureMethodIsNotUsed - TopologicalSort will throw if the graph has a cycle.
        var sorted =
            placeholders
                .Select(x => (x.Key, x.Value))
                .ToDirectedGraph()
                .TopologicalSort(StringComparer.OrdinalIgnoreCase)
                .ToList();

        var missingPlaceholders = sorted.Where(node => !placeholders.ContainsKey(node)).ToList();
        if (missingPlaceholders.Any())
        {
            throw DynamicException.Create("MissingPlaceholder", $"One or more placeholders are missing: [{string.Join(", ", missingPlaceholders)}]");
        }

        return replacements;
    }
}