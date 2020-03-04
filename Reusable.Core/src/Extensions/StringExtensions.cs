using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.Extensions
{
    public static class StringExtensions
    {
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

        [Obsolete]
        public static string Stringify<T>(this T obj)
        {
            return obj?.ToString().QuoteWith("'") ?? string.Empty;
        }

        [ContractAnnotation("text: null => halt")]
        public static string CapitalizeFirstLetter(this string text)
        {
            return Regex.Replace(text, "^[a-z]", m => m.Value.ToUpper());
        }

        public static IEnumerable<string> SplitByLineBreaks(this string text, bool ignoreEmptyEntries = true)
        {
            return
                from line in Regex.Split(text, @"(\r\n|\r|\n)")
                where !ignoreEmptyEntries || line.Trim().IsNotNullOrEmpty()
                select line;
        }

        public static bool Contains(this string value, string other, StringComparison comparisonType = StringComparison.OrdinalIgnoreCase)
        {
            return value.IndexOf(other, comparisonType) >= 0;
        }

        [ContractAnnotation("value: null => null; notnull => notnull")]
        public static SoftString? ToSoftString(this string? value) => value is {} ? SoftString.Create(value) : default;

        [NotNull, ContractAnnotation("value: null => halt; encoding: null => halt")]
        public static StreamReader ToStreamReader(this string value, [NotNull] Encoding encoding)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (encoding == null) throw new ArgumentNullException(nameof(encoding));

            return new StreamReader(new MemoryStream(encoding.GetBytes(value)));
        }

        [NotNull, ContractAnnotation("value: null => halt; value: notnull => notnull")]
        public static StreamReader ToStreamReader([NotNull] this string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            return value.ToStreamReader(Encoding.UTF8);
        }

        [NotNull, ContractAnnotation("value: null => halt; value: notnull => notnull")]
        public static Stream ToStream(this string value, Encoding? encoding = default)
        {
            return new MemoryStream((encoding ?? Encoding.UTF8).GetBytes(value));
        }

        public static bool Matches(this string? value, [RegexPattern] string pattern, RegexOptions options = RegexOptions.None)
        {
            return value is {} && Regex.IsMatch(value, pattern, options);
        }

        public static bool SoftEquals(this string? value, string? other) => SoftString.Comparer.Equals(value, other);

        public static string ToCamelCase(this string? value) => value is {} ? Regex.Replace(value, @"\A([A-Z]+)", m => m.Value.ToLower()) : string.Empty;
    }
}