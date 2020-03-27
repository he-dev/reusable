using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Exceptionize;

namespace Reusable.Extensions
{
    public delegate bool TryGetValueCallback(string name, out object? value);

    public interface ITryGetValue<in TKey, TValue>
    {
        bool Execute(TKey key, out TValue value);
    }

    public interface ITryGetFormatValue : ITryGetValue<string, object?> { }

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
        public static string Format(this string? text, ITryGetFormatValue tryGetFormatValue, IFormatProvider? formatProvider = default)
        {
            return text.Format((string name, out object? value) => tryGetFormatValue.Execute(name, out value), formatProvider);
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
        public static string Format(this string? text, [NotNull] IDictionary<SoftString, object> args)
        {
            return Format(text, (string name, out object value) => args.TryGetValue(name, out value));
        }

        [MustUseReturnValue]
        [ContractAnnotation("text: null => null; args: null => stop")]
        public static string Format(this string? text, object args, IEqualityComparer<string> comparer, IFormatProvider formatProvider)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                return text;
            }

            var properties =
                args
                    .GetType()
                    .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    //.Where(p => p.IsDefined(typeof(IgnoreAttribute))) // todo - add ignore attribute
                    .ToDictionary(p => p.Name, p => p, comparer);

            return Format(text, (string name, out object value) =>
            {
                if (properties.TryGetValue(name, out var property))
                {
                    value = property.GetValue(args);
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
        [NotNull, ItemNotNull]
        public static IEnumerable<string> GetNames(string text)
        {
            return
                string.IsNullOrEmpty(text)
                    ? Enumerable.Empty<string>()
                    : Regex.Matches(text, ExpressionPattern).Cast<Match>().Select(m => m.Groups[Groups.Name].Value);
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

        //public static string ToJson<TException>(
        //    this TException exception,
        //    IEnumerable<PropertyInfo> excludeProperties = null,
        //    Formatting formatting = Formatting.Indented) where TException : Exception
        //{
        //    excludeProperties = excludeProperties ?? typeof(Exception).GetProperties(BindingFlags.Instance | BindingFlags.Public);
        //    var exceptionInfos = exception.GetInnerExceptions().Select(ex => CreateExceptionInfo(ex, excludeProperties));
        //    var json = JsonConvert.SerializeObject(exceptionInfos, formatting);
        //    return json;
        //}

        //public static string ToDebugString(this object data, IEnumerable<string> exclude = null, IFormatProvider formatProvider = null)
        //{
        //    exclude = exclude ?? Enumerable.Empty<string>();
        //    formatProvider = formatProvider ?? CultureInfo.InvariantCulture;
        //    var properties = data.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public).IsTrue(p => !exclude.Contains(p.Name));
        //    var strings = properties.Select(p => p.Name + " = \"" + string.Format(formatProvider, "{0}", p.GetValue(data)) + "\"");
        //    var result = string.Join(" ", strings);
        //    return result; // asObject ? $"{{{result}}}" : result;
        //}
    }
}