using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Reusable.Formatters;

namespace Reusable
{
    public static class StringInterpolation
    {
        public static string Format(this string str, Func<string, object> getValueOrDefault, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrEmpty(str)) { return str; }
            if (getValueOrDefault == null) { throw new ArgumentNullException(nameof(getValueOrDefault)); }

            formatProvider = formatProvider ?? CustomFormatter.Default(); // new DefaultFormatter();

            // https://regex101.com/r/sK1tS8/5
            var result = Regex.Replace(str, "(?<!{){(?<name>[a-zA-Z_][a-zA-Z0-9_]*)(,(?<alignment>-?\\d+))?(:(?<formatString>.*?))?}(?!})", match =>
            {
                var name = match.Groups["name"].Value;
                var value = getValueOrDefault(name);
                if (value == null)
                {
                    return "{" + name + "}";
                }
                var alignment = match.Groups["alignment"].Success ? "," + match.Groups["alignment"].Value : string.Empty;
                var formatString = match.Groups["formatString"].Success ? ":" + match.Groups["formatString"].Value : string.Empty;
                return string.Format(formatProvider, "{0" + alignment + formatString + "}", value);
            });

            // https://regex101.com/r/zG6tF7/3
            result = Regex.Replace(result, "{{(?<contents>.+?)}}", match => "{" + match.Groups["contents"].Value + "}");

            return result;
        }

        public static string Format(this string str, IDictionary<string, object> data, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrEmpty(str)) { return str; }
            if (data == null) { throw new ArgumentNullException(nameof(data)); }

            return Format(str, name =>
            {
                var value = (object)null;
                return data.TryGetValue(name, out value) ? value : null;
            }, formatProvider);
        }

        public static string Format(this string str, object data, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrEmpty(str)) { return str; }
            if (data == null) { throw new ArgumentNullException(nameof(data)); }

            var properties = data.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance).ToDictionary(p => p.Name, p => p);
            return Format(str, name =>
            {
                var property = (PropertyInfo)null;
                return properties.TryGetValue(name, out property) ? property.GetValue(data) : null;
            }, formatProvider);
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
