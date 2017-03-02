using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Reusable.StringFormatting;

namespace Reusable
{
    public static class StringInterpolation
    {
        private static readonly string PlaceholderPattern = "(?<!{){(?<Name>[a-zA-Z_][a-zA-Z0-9_]*)(,(?<Alignment>-?\\d+))?(:(?<FormatString>.*?))?}(?!})";

        private static class GroupName
        {
            public const string Name = nameof(Name);
            public const string Alignment = nameof(Alignment);
            public const string FormatString = nameof(FormatString);
        }

        public static string Format(this string text, Func<string, object> getValueOrDefault, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrEmpty(text)) { return text; }
            if (getValueOrDefault == null) { throw new ArgumentNullException(nameof(getValueOrDefault)); }

            formatProvider = formatProvider ?? new DefaultFormatter();

            // https://regex101.com/r/sK1tS8/5
            var result = Regex.Replace(text, PlaceholderPattern, match =>
            {
                var name = match.Groups[GroupName.Name].Value;
                var value = getValueOrDefault(name);
                if (value == null)
                {
                    return $"{{{name}}}";
                }
                var alignment = match.Groups[GroupName.Alignment].Success ? "," + match.Groups[GroupName.Alignment].Value : string.Empty;
                var formatString = match.Groups[GroupName.FormatString].Success ? ":" + match.Groups[GroupName.FormatString].Value : string.Empty;
                return string.Format(formatProvider, $"{{0{alignment}{formatString}}}", value);
            });

            // https://regex101.com/r/zG6tF7/3
            result = Regex.Replace(result, "{{(?<contents>.+?)}}", match => $"{{{match.Groups["contents"].Value}}}");

            return result;
        }

        public static string Format(this string text, IDictionary<string, object> data, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrEmpty(text)) { return text; }
            if (data == null) { throw new ArgumentNullException(nameof(data)); }

            return Format(text, name =>
            {
                return data.TryGetValue(name, out object value) ? value : null;
            }, formatProvider);
        }

        public static string Format(this string text, object data, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrEmpty(text)) { return text; }
            if (data == null) { throw new ArgumentNullException(nameof(data)); }

            var properties = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, p => p);
            return Format(text, name =>
            {
                return properties.TryGetValue(name, out PropertyInfo property) ? property.GetValue(data) : null;
            }, formatProvider);
        }

        public static string FormatAll(this string text, IDictionary<string, object> data, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrEmpty(text)) { return text; }
            if (data == null) { throw new ArgumentNullException(nameof(data)); }

            formatProvider = formatProvider ?? new DefaultFormatter();

            var dependencies = data.ToDictionary(x => x.Key, x => GetNames(string.Format(formatProvider, "{0}", x.Value)));
            Validator.ValidateDependencies(dependencies);

            while (text.ToString() != (text = text.Format(data, formatProvider))) ;
            return text;
        }

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

        public static string GetConnectionStringName(this string value)
        {
            if (string.IsNullOrEmpty(value)) { throw new ArgumentNullException(nameof(value)); }

            return Regex
                .Match(value, "^name=(?<connectionStringName>.+)", RegexOptions.IgnoreCase)
                .Groups["connectionStringName"]
                .Value;
        }
    }
}
