using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;

namespace Reusable
{
    public static class StringExtensions
    {
        public static Tuple<bool, Tuple<T1, T2>> Parse<T1, T2>(this string input, string pattern, RegexOptions options = RegexOptions.None)
        {
            return input.Parse(pattern, options).Tupleize<T1, T2>();
        }

        public static Tuple<bool, Tuple<T1, T2, T3>> Parse<T1, T2, T3>(this string input, string pattern, RegexOptions options = RegexOptions.None)
        {
            return input.Parse(pattern, options).Tupleize<T1, T2, T3>();
        }

        public static Tuple<bool, Tuple<T1, T2, T3, T4>> Parse<T1, T2, T3, T4>(this string input, string pattern, RegexOptions options = RegexOptions.None)
        {
            return input.Parse(pattern, options).Tupleize<T1, T2, T3, T4>();
        }

        public static Tuple<bool, Tuple<T1, T2, T3, T4, T5>> Parse<T1, T2, T3, T4, T5>(this string input, string pattern, RegexOptions options = RegexOptions.None)
        {
            return input.Parse(pattern, options).Tupleize<T1, T2, T3, T4, T5>();
        }

        public static Tuple<bool, Tuple<T1, T2, T3, T4, T5, T6>> Parse<T1, T2, T3, T4, T5, T6>(this string input, string pattern, RegexOptions options = RegexOptions.None)
        {
            return input.Parse(pattern, options).Tupleize<T1, T2, T3, T4, T5, T6>();
        }

        public static Tuple<bool, Tuple<T1, T2, T3, T4, T5, T6, T7>> Parse<T1, T2, T3, T4, T5, T6, T7>(this string input, string pattern, RegexOptions options = RegexOptions.None)
        {
            return input.Parse(pattern, options).Tupleize<T1, T2, T3, T4, T5, T6, T7>();
        }

        private static IDictionary<int, object> Parse(this string input, string pattern, RegexOptions options)
        {
            var inputMatch = Regex.Match(input, pattern, RegexOptions.ExplicitCapture | options);

            var result =
                inputMatch.Success
                    ? inputMatch
                        .Groups
                        .Cast<Group>()
                        .Skip(1) // The first group is the entire match that we don't need.
                        .Where(g => g.Success)
                        .Select(
                            g =>
                            {
                                var ordinal = Regex.Match(g.Name, @"^(?:T(?<ordinal>\d+))").Groups["ordinal"];
                                return
                                (
                                    Ordinal: ordinal.Success && int.TryParse(ordinal.Value, out var x) && x > 0 ? x : throw DynamicException.Create("InvalidTypeIndex", $"Type index '{g.Name}' must begin with the upper-case 'T' and be followed by a 1 based index, e.g. 'T1'."),
                                    Value: string.IsNullOrEmpty(g.Value) ? null : g.Value
                                );
                            }
                        )
                        .Where(g => g.Value.IsNotNull())
                        .ToDictionary
                        (
                            g => g.Ordinal,
                            g => (object)g.Value
                        )
                    : new Dictionary<int, object>();

            result[Tupleizer.SuccessKey] = inputMatch.Success;

            return result;
        }

        // ----------------

        public static string IndentLines([NotNull] this string value, int indentWidth, char indentCharacter = ' ', Encoding? encoding = default)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
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

        public static string ConcatIfNotEmpty(this string str, string value, string separator = " ")
        {
            return
                value.IsNullOrEmpty()
                    ? str
                    : str + separator + value;
        }
        
        
    }
}