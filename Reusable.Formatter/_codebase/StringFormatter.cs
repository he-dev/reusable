using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json;
using SmartUtilities.Frameworks.AnyFormat;
using SmartUtilities.Frameworks.AnyFormat.Formatters;
using SmartUtilities.Frameworks.InlineValidation;
using SmartUtilities.Frameworks.InlineValidation.Validations;

namespace SmartUtilities
{
    public static class StringFormatter
    {
        public static string Format(this string str, Func<string, object> getValueOrDefault, IFormatProvider formatProvider = null)
        {
            if (string.IsNullOrEmpty(str)) { return str; }
            if (getValueOrDefault == null) { throw new ArgumentNullException(nameof(getValueOrDefault)); }

            formatProvider = formatProvider ?? Formatter.Default();

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

        public static string ToShortString(this Type type)
        {
            var codeDomProvider = CodeDomProvider.CreateProvider("C#");
            var typeReferenceExpression = new CodeTypeReferenceExpression(type);
            using (var writer = new StringWriter())
            {
                codeDomProvider.GenerateCodeFromExpression(typeReferenceExpression, writer, new CodeGeneratorOptions());
                return writer.GetStringBuilder().ToString();
            }
        }

        public static string ToShortString(this MethodInfo methodInfo)
        {
            methodInfo.Validate(nameof(methodInfo)).IsNotNull();

            var signature =
                new StringBuilder()
                .Append(methodInfo.ReturnType.FullName)
                .Append(" ").Append(methodInfo.DeclaringType?.FullName)
                .Append(".").Append(methodInfo.Name)
                .Append("(")
                .Append(string.Join(", ", methodInfo.GetParameters().Select(p => $"{p.ParameterType.ToShortString()} {p.Name}")))
                .Append(")")
                .ToString();

            return signature;
        }

        public static string ToJson<TException>(
            this TException exception, 
            IEnumerable<PropertyInfo> excludeProperties = null, 
            Formatting formatting = Formatting.Indented) where TException : Exception
        {
            excludeProperties = excludeProperties ?? typeof(Exception).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            var exceptionInfos = exception.InnerExceptions().Select(ex => CreateExceptionInfo(ex, excludeProperties));
            var json = JsonConvert.SerializeObject(exceptionInfos, formatting);
            return json;
        }

        private static dynamic CreateExceptionInfo<TException>(TException exception, IEnumerable<PropertyInfo> excludeProperties)
            where TException : Exception
        {
            var exceptionInfo = new ExpandoObject() as dynamic;

            exceptionInfo.Exception = Regex.Replace(exception.GetType().Name, "Exception$", string.Empty, RegexOptions.IgnoreCase);
            exceptionInfo.Message = exception.Message;

            // Get exception properties.
            exceptionInfo.Properties = new ExpandoObject() as dynamic;

            var exceptionProperties =
                exception.GetType()
                .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .Except(excludeProperties, new PropertyInfoEqualityComparer());

            foreach (var property in exceptionProperties)
            {
                ((IDictionary<string, object>)exceptionInfo.Properties)[property.Name] = property.GetValue(exception);
            }

            // Create strack trace.
            var strackTrace = new StackTrace(exception, true);
            var stackFrames = strackTrace.GetFrames();

            exceptionInfo.StackTrace = stackFrames?.Select(sf =>
            {
                var stackFrame = new ExpandoObject() as dynamic;
                stackFrame.Caller = (sf.GetMethod() as MethodInfo)?.ToShortString() ?? "<MethodInfo not found>";
                stackFrame.FileName = sf.GetFileName();
                stackFrame.LineNumber = sf.GetFileLineNumber();
                return stackFrame;
            }).ToList();

            return exceptionInfo;
        }

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

    internal class PropertyInfoEqualityComparer : IEqualityComparer<PropertyInfo>
    {
        public bool Equals(PropertyInfo x, PropertyInfo y)
        {
            return
                !ReferenceEquals(x, null) &&
                !ReferenceEquals(y, null) &&
                x.Name == y.Name;
        }

        public int GetHashCode(PropertyInfo obj)
        {
            return obj.Name.GetHashCode();
        }
    }
}
