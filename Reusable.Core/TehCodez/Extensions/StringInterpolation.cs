using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.StringFormatting;

namespace Reusable.Extensions
{    
    public delegate bool TryGetValueCallback(string name, out object value);

    public static class StringInterpolation
    {
        private static readonly string PlaceholderPattern = "(?<!{){(?<Name>[a-zA-Z_][a-zA-Z0-9_.-]*)(,(?<Alignment>-?\\d+))?(:(?<FormatString>.*?))?}(?!})";

        private static class GroupName
        {
            public const string Name = nameof(Name);
            public const string Alignment = nameof(Alignment);
            public const string FormatString = nameof(FormatString);
        }

        [Pure]
        [CanBeNull]
        [ContractAnnotation("text: null => null; text: notnull => notnull; tryGetValue: null => stop")]
        public static string Format(this string text, TryGetValueCallback tryGetValue, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrEmpty(text)) { return text; }
            if (tryGetValue == null) { throw new ArgumentNullException(nameof(tryGetValue)); }

            formatProvider = formatProvider ?? new DefaultFormatter();

            // https://regex101.com/r/sK1tS8/5
            var result = Regex.Replace(text, PlaceholderPattern, match =>
            {
                var name = match.Groups[GroupName.Name].Value;
                var alignment = match.Groups[GroupName.Alignment].Success ? "," + match.Groups[GroupName.Alignment].Value : string.Empty;
                var formatString = match.Groups[GroupName.FormatString].Success ? ":" + match.Groups[GroupName.FormatString].Value : string.Empty;

                if (tryGetValue(name, out object value))
                {
                    // Apply formatting.
                    return string.Format(formatProvider, $"{{0{alignment}{formatString}}}", value);
                }
                else
                {
                    // Reconstruct the format string.
                    return $"{{{name}{alignment}{formatString}}}";
                }
            });

            // https://regex101.com/r/zG6tF7/3
            result = Regex.Replace(result, "{{(?<contents>.+?)}}", match => $"{{{match.Groups["contents"].Value}}}");

            return result;
        }

        [Pure]
        [CanBeNull]
        [ContractAnnotation("text: null => null; data: null => stop")]
        public static string Format(this string text, IDictionary<string, object> data, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrEmpty(text)) { return text; }
            if (data == null) { throw new ArgumentNullException(nameof(data)); }

            return Format(text, data.TryGetValue, formatProvider);
        }

        [Pure]
        [CanBeNull]
        [ContractAnnotation("text: null => null; data: null => stop")]
        public static string Format(this string text, object data, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrEmpty(text)) { return text; }
            if (data == null) { throw new ArgumentNullException(nameof(data)); }

            var properties = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, p => p);
            return Format(text, (string name, out object value) =>
            {
                if (properties.TryGetValue(name, out PropertyInfo property))
                {
                    value = property.GetValue(data);
                    return true;
                }
                else
                {
                    value = null;
                    return false;
                }
            }, formatProvider);
        }

        [Pure]
        [CanBeNull]
        [ContractAnnotation("text: null => null; data: null => stop")]
        public static string FormatAll(this string text, IDictionary<string, object> data, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrEmpty(text)) { return text; }
            if (data == null) { throw new ArgumentNullException(nameof(data)); }

            formatProvider = formatProvider ?? new DefaultFormatter();

            var dependencies = data.ToDictionary(x => x.Key, x => GetNames(string.Format(formatProvider, "{0}", x.Value)));
            DependencyValidator.ValidateDependencies(dependencies);

            while (text.ToString() != (text = text.Format(data, formatProvider))) ;
            return text;
        }

        [Pure]
        [NotNull]
        [ItemNotNull]
        public static IEnumerable<string> GetNames(string text)
        {
            return
                string.IsNullOrEmpty(text)
                    ? Enumerable.Empty<string>()
                    : Regex.Matches(text, PlaceholderPattern).Cast<Match>().Select(m => m.Groups[GroupName.Name].Value);
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
