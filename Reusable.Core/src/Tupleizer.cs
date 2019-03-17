using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Reflection;

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
            if (string.IsNullOrEmpty(input)) throw new ArgumentException($"{nameof(input)} must not be null or empty.");
            if (string.IsNullOrEmpty(pattern)) throw new ArgumentException($"{nameof(pattern)} must not be null or empty.");

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
                        .ToDictionary(
                            g => g.Ordinal,
                            g => (object)g.Value
                        )
                    : new Dictionary<int, object>();

            result[Tupleizer.SuccessKey] = inputMatch.Success;

            return result;
        }
    }

    internal static class Tupleizer
    {
        public const int SuccessKey = 0;

        public static Tuple<bool, Tuple<T1, T2>> Tupleize<T1, T2>(this IDictionary<int, object> data)
        {
            return
                Tuple.Create(
                    data.GetValue<bool>(SuccessKey),
                    Tuple.Create(
                        data.GetValue<T1>(1),
                        data.GetValue<T2>(2)
                    )
                );
        }

        public static Tuple<bool, Tuple<T1, T2, T3>> Tupleize<T1, T2, T3>(this IDictionary<int, object> data)
        {
            return
                Tuple.Create(
                    data.GetValue<bool>(SuccessKey),
                    Tuple.Create(
                        data.GetValue<T1>(1),
                        data.GetValue<T2>(2),
                        data.GetValue<T3>(3)
                    )
                );
        }

        public static Tuple<bool, Tuple<T1, T2, T3, T4>> Tupleize<T1, T2, T3, T4>(this IDictionary<int, object> data)
        {
            return
                Tuple.Create(
                    data.GetValue<bool>(SuccessKey),
                    Tuple.Create(
                        data.GetValue<T1>(1),
                        data.GetValue<T2>(2),
                        data.GetValue<T3>(3),
                        data.GetValue<T4>(4)
                    )
                );
        }

        public static Tuple<bool, Tuple<T1, T2, T3, T4, T5>> Tupleize<T1, T2, T3, T4, T5>(this IDictionary<int, object> data)
        {
            return
                Tuple.Create(
                    data.GetValue<bool>(SuccessKey),
                    Tuple.Create(
                        data.GetValue<T1>(1),
                        data.GetValue<T2>(2),
                        data.GetValue<T3>(3),
                        data.GetValue<T4>(4),
                        data.GetValue<T5>(5)
                    )
                );
        }

        public static Tuple<bool, Tuple<T1, T2, T3, T4, T5, T6>> Tupleize<T1, T2, T3, T4, T5, T6>(this IDictionary<int, object> data)
        {
            return
                Tuple.Create(
                    data.GetValue<bool>(SuccessKey),
                    Tuple.Create(
                        data.GetValue<T1>(1),
                        data.GetValue<T2>(2),
                        data.GetValue<T3>(3),
                        data.GetValue<T4>(4),
                        data.GetValue<T5>(5),
                        data.GetValue<T6>(6)
                    )
                );
        }

        public static Tuple<bool, Tuple<T1, T2, T3, T4, T5, T6, T7>> Tupleize<T1, T2, T3, T4, T5, T6, T7>(this IDictionary<int, object> data)
        {
            return
                Tuple.Create(
                    data.GetValue<bool>(SuccessKey),
                    Tuple.Create(
                        data.GetValue<T1>(1),
                        data.GetValue<T2>(2),
                        data.GetValue<T3>(3),
                        data.GetValue<T4>(4),
                        data.GetValue<T5>(5),
                        data.GetValue<T6>(6),
                        data.GetValue<T7>(7)
                    )
                );
        }

        private static T GetValue<T>(this IDictionary<int, object> data, int key)
        {
            if (data.TryGetValue(key, out var value))
            {
                var valueType = typeof(T);
                var targetType =
                    valueType.IsNullable()
                        ? valueType.GetGenericArguments().Single()
                        : valueType;

                return (T)Convert.ChangeType(value, targetType);
            }
            else
            {
                return default;
            }
        }
    }
}