using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Text.RegularExpressions;
using Reusable.Extensions;

namespace Reusable.Tests.XUnit.Experimental
{
    public enum CronExtension
    {
        None,
        First,
        Second,
        Third,
        Fourth,
        Fifth,
        Last,
        Weekday,
        L = Last,
        W = Weekday
    }

    public interface ICronField
    {
        bool Contains(DateTime dateTime);
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class CronField : ICronField
    {
        private const string Pattern = @"(?<min>{fieldPattern})(?:-(?<max>{fieldPattern}))?(?:\/(?<step>\d+))?((?:#(?<nth>[1-5]))|(?<ext>L|W))?";

        protected CronField(int? min, int? max, int? step, CronExtension extension)
        {
            var checkRange = min.HasValue && max.HasValue;
            if (checkRange)
            {
                var isValidRange = min.Value < max.Value;
                if (!isValidRange)
                {
                    throw new ArgumentException($"{nameof(min)} must be less then {nameof(max)}.");
                }
            }

            Min = min;
            Max = max;
            Step = step;
            Extension = extension;
        }

        public static CronField Any = new CronAny();

        public int? Min { get; }

        public int? Max { get; }

        public int? Step { get; }

        public CronExtension? Extension { get; }

        private string DebuggerDisplay => ToString();

        private static readonly ConcurrentDictionary<Type, string> FieldPatternCache = new ConcurrentDictionary<Type, string>();

        internal static bool TryParse<T>(string input, out CronField cronField) where T : CronField
        {
            if (input == Any)
            {
                cronField = Any;
                return true;
            }

            var fieldValues = DuckObject<T>.Quack<IReadOnlyDictionary<string, int>>(duck => duck.ValueMap);
            var fieldPattern = FieldPatternCache.GetOrAdd(typeof(T), t => fieldValues.Select(x => Regex.Escape(x.Key)).OrderByDescending(x => x).Join("|"));

            var match = Regex.Match(input, Pattern.Format(new { fieldPattern }), RegexOptions.IgnoreCase);
            if (match.Success)
            {
                var min = default(int?);
                var max = default(int?);
                var step = default(int?);
                var extension = CronExtension.None;

                if (fieldValues.TryGetValue(match.Groups["min"].Value, out var _min))
                {
                    min = _min;
                }
                else
                {
                    cronField = default;
                    return false;
                }

                if (match.Groups["max"].Success && fieldValues.TryGetValue(match.Groups["max"].Value, out var _max))
                {
                    max = _max;
                }

                if (match.Groups["step"].Success && int.TryParse(match.Groups["step"].Value, out var _step))
                {
                    step = _step;
                }

                if (match.Groups["ext"].Success && Enum.TryParse<CronExtension>(match.Groups["ext"].Value, out var ext)) { }

                cronField = (T)Activator.CreateInstance(typeof(T), min, max, step, extension);
                return true;
            }

            cronField = default;
            return false;
        }

        protected abstract int GetValue(DateTime point);

        public virtual bool Contains(DateTime timestamp)
        {
            var value = GetValue(timestamp);

            var isExact = Min.HasValue && !Max.HasValue && !Step.HasValue;
            if (isExact && value != Min.Value)
            {
                return false;
            }

            var isRange = Min.HasValue && Max.HasValue;

            if (isRange && !(value > Min.Value && value < Max.Value))
            {
                return false;
            }

            if (value > Min.Value)
            {
                value -= Min.Value;
            }

            if (Step.HasValue && value % Step.Value != 0)
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            var min = Min;
            var max = Max.HasValue && Max > Min ? $"-{Max}" : string.Empty;
            var step = Step.HasValue ? $"/{Step}" : string.Empty;

            return $"{min}{max}{step}";
        }

        public static implicit operator string(CronField cronField) => cronField?.ToString();
    }

    public static class CronExpressionExtensions
    {
        // Replaces multiple whitespaces by a single one, trims the string and makes it uppercase.
        public static string NormalizeCronExpression(this string input)
        {
            return Regex.Replace(input, @"\s+", " ").Trim().ToUpper();
        }
    }

    public class CronAny : CronField
    {
        public CronAny()
            : base(null, null, null, CronExtension.None) { }

        protected override int GetValue(DateTime point) => -1;

        public override bool Contains(DateTime dateTime) => true;

        public override string ToString() => "*";
    }

    public class CronSecond : CronField
    {
        public static readonly IReadOnlyDictionary<string, int> ValueMap;

        static CronSecond()
        {
            ValueMap =
                Enumerable
                    .Range(0, 60)
                    .ToDictionary(x => x.ToString(), x => x);
        }

        public CronSecond(int? min, int? max, int? step, CronExtension extension)
            : base(min, max, step, extension) { }

        protected override int GetValue(DateTime point) => point.Second;
    }

    public class CronMinute : CronField
    {
        public static readonly IReadOnlyDictionary<string, int> ValueMap;

        static CronMinute()
        {
            ValueMap =
                Enumerable
                    .Range(0, 60)
                    .ToDictionary(x => x.ToString(), x => x);
        }

        public CronMinute(int? min, int? max, int? step, CronExtension extension)
            : base(min, max, step, extension) { }

        protected override int GetValue(DateTime point) => point.Minute;
    }

    public class CronHour : CronField
    {
        public static readonly IReadOnlyDictionary<string, int> ValueMap;

        static CronHour()
        {
            ValueMap =
                Enumerable
                    .Range(0, 24)
                    .ToDictionary(x => x.ToString(), x => x);
        }

        public CronHour(int? min, int? max, int? step, CronExtension extension)
            : base(min, max, step, extension) { }

        protected override int GetValue(DateTime point) => point.Hour;
    }

    public class CronDayOfMonth : CronField
    {
        public static readonly IReadOnlyDictionary<string, int> ValueMap;

        static CronDayOfMonth()
        {
            ValueMap =
                Enumerable
                    .Range(1, 31)
                    .ToDictionary(x => x.ToString(), x => x);
        }

        public CronDayOfMonth(int? min, int? max, int? step, CronExtension extension)
            : base(min, max, step, extension) { }

        protected override int GetValue(DateTime point) => point.Day;
    }

    public class CronMonth : CronField
    {
        public static readonly IReadOnlyDictionary<string, int> ValueMap;

        static CronMonth()
        {
            ValueMap =
                Enumerable
                    .Range(0, 12)
                    .Select(x => (key: x.ToString(), value: x))
                    .Concat(new[]
                        {
                            "JAN", "FEB", "MAR", "APR", "MAI", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"
                        }
                        .Select((dayOfWeek, index) => (key: dayOfWeek, value: index)))
                    .ToDictionary(x => x.key, x => x.value + 1, StringComparer.OrdinalIgnoreCase);
        }

        public CronMonth(int? min, int? max, int? step, CronExtension extension)
            : base(min, max, step, extension) { }

        protected override int GetValue(DateTime point) => point.Month;
    }

    public class CronDayOfWeek : CronField
    {
        public static readonly IReadOnlyDictionary<string, int> ValueMap;

        static CronDayOfWeek()
        {
            ValueMap =
                Enumerable
                    .Range(1, 7)
                    .Select(x => (key: x.ToString(), value: x))
                    .Concat(new[]
                        {
                            "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"
                        }
                        .Select((dayOfWeek, index) => (key: dayOfWeek, value: index)))
                    .ToDictionary(x => x.key, x => x.value + 1, StringComparer.OrdinalIgnoreCase);
        }

        public CronDayOfWeek(int? min, int? max, int? step, CronExtension extension)
            : base(min, max, step, extension) { }

        protected override int GetValue(DateTime point) => (int)point.DayOfWeek + 1;
    }

    public class CronYear : CronField
    {
        public static readonly IReadOnlyDictionary<string, int> ValueMap;

        static CronYear()
        {
            ValueMap =
                Enumerable
                    .Range(1970, 130)
                    .ToDictionary(x => x.ToString(), x => x);
        }

        public CronYear(int? min, int? max, int? step, CronExtension extension)
            : base(min, max, step, extension) { }

        protected override int GetValue(DateTime point) => point.Year;
    }

    internal delegate bool TryParseFunc(string value, out CronField result);

    public interface ICronExpression
    {
        bool Contains(DateTime timestamp);
    }

    public class CronExpression : ICronExpression, IEnumerable<IEnumerable<CronField>>
    {
        private static readonly IEnumerable<TryParseFunc> TryParseCronFields = new TryParseFunc[]
        {
            CronField.TryParse<CronSecond>,
            CronField.TryParse<CronMinute>,
            CronField.TryParse<CronHour>,
            CronField.TryParse<CronDayOfMonth>,
            CronField.TryParse<CronMonth>,
            CronField.TryParse<CronDayOfWeek>,
            CronField.TryParse<CronYear>,
        };

        private readonly IEnumerable<IEnumerable<CronField>> _cronFields;

        private CronExpression(IEnumerable<IEnumerable<CronField>> cronFields)
        {
            _cronFields = cronFields;
        }

        public static bool TryParse(string input, out CronExpression result)
        {
            var cronFields =
                input
                    .NormalizeCronExpression()
                    .Split(' ')
                    .Select(x => x.Split(','))
                    .Zip(
                        TryParseCronFields,
                        (fieldValues, tryParseCronField) => fieldValues
                            .Select(value =>
                                tryParseCronField(value, out var cronField)
                                    ? cronField
                                    : default
                            )
                    )
                    .Select(g => g
                        .Where(Conditional.IsNotNull)
                        .OrderBy(x => x.Min ?? 0)
                        .ThenBy(x => x.Max ?? 0)
                        .ThenBy(x => x.Step ?? 0)
                        .ToList()
                    )
                    .ToList();

            if (cronFields.Any())
            {
                result = new CronExpression(cronFields);
                return true;
            }

            result = default;
            return false;
        }

        public static CronExpression Parse(string input)
        {
            return TryParse(input, out var result) ? result : throw new ArgumentException("Invalid Cron-Expression.");
        }

        public bool Contains(DateTime timestamp) => this.All(x => x.Any(f => f.Contains(timestamp)));

        public IEnumerator<IEnumerable<CronField>> GetEnumerator() => _cronFields.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => this.Select(g => g.Select(f => f.ToString()).Join(",")).Join(" ");

        // LINQPad
        private object ToDump() => ToString();
    }
}