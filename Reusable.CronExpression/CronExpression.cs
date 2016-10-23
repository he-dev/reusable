using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Subexpressions;

namespace Reusable
{
    public class CronExpression
    {
        private readonly IEnumerable<CronSubexpression> _subexpressions;

        private CronExpression(params CronSubexpression[] subexpressions)
        {
            _subexpressions = subexpressions;
        }

        public static CronExpression Parse(string expression)
        {
            var fields = expression.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return new CronExpression(
                new CronSecond(fields[0]),
                new CronMinute(fields[1]),
                new CronHour(fields[2])
            );
        }

        public bool IsMatch(DateTime dateTime)
        {
            return _subexpressions.All(expr => expr.IsMatch(dateTime));
        }
    }

    public enum CronSuffixType
    {
        Undefined,
        Step,
        Last,
        Week,
        Ordinal
    }

    public class CronValue
    {
        private int[] _range;

        public CronValue(int min, int max) { Min = min; Max = max; }

        public int Min { get; }

        public int Max { get; }

        public int[] Range
        {
            get { return _range; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(Range));
                }

                // It's easier to use it when there are always two elements.
                _range = value.Count() == 1 ? Enumerable.Repeat(value.First(), 2).ToArray() : value;

                if (!_range.Any()) { return; }

                if (!_range.All(x => Min <= x && x <= Max))
                {
                    throw new ArgumentOutOfRangeException($"Range must be greater or equal {Min} and less or equal {Max}.");
                }

                if (!_range.SequenceEqual(_range.OrderBy(x => x)))
                {
                    throw new ArgumentException("Range must be in ascending order.");
                }
            }
        }

        public int? Suffix { get; set; }

        public CronSuffixType SuffixType { get; set; }

        public bool Contains(int value)
        {
            return _range[0] <= value && value <= _range[1];
        }
    }

    public abstract class CronSubexpression
    {
        private static class Pattern
        {
            public static readonly string[] Values =
            {
                "(?<any>\\*)",
                "(?<range>[0-9]+\\-[0-9]+|[a-z]{3}\\-[a-z]{3})",
                "(?<absolute>[0-9]+|[a-z]{3})",
            };

            public static readonly string[] Suffixes =
            {
                "(\\/(?<step>[0-9]+))",
                "(?<last>L)",
                "(?<week>W)",
                "(?<ordinal>#[1-5])",
            };
        }

        private static readonly Regex Matcher = new Regex(
            $"^({string.Join("|", Pattern.Values)})({string.Join("|", Pattern.Suffixes)}$)?",
            RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture
        );

        protected CronSubexpression(string expression, int min, int max, Type nameType = null)
        {
            Min = min;
            Max = max;
            NameType = nameType;
            Values = expression.Split(',').Select(Parse).ToList();
        }

        protected int Min { get; }

        protected int Max { get; }

        protected Type NameType { get; }

        protected List<CronValue> Values { get; }

        private CronValue Parse(IEnumerable<string> values, string suffix, CronSuffixType suffixType)
        {
            return new CronValue(Min, Max)
            {
                Range = values.Select(x =>
                {
                    var value = 0;
                    return int.TryParse(x, out value) ? value : (int)Enum.Parse(NameType, suffix, true);
                }).ToArray(),
                Suffix =
                    suffixType == CronSuffixType.Step || suffixType == CronSuffixType.Ordinal
                    ? int.Parse(suffix)
                    : (int?)null,
                SuffixType = suffixType
            };
        }

        public bool IsMatch(DateTime dateTime)
        {
            return Values.All(x => x.Contains(dateTime.Minute) && IsMatch(dateTime, x.Suffix, x.SuffixType));
        }

        protected abstract bool IsMatch(DateTime dateTime, int? suffix, CronSuffixType suffixType);

        private CronValue Parse(string expression)
        {
            var match = Matcher.Match(expression);

            var values = (string[])null;
            var suffix = (string)null;
            var suffixType = CronSuffixType.Undefined;

            if (match.Groups["absolute"].Success)
            {
                values = new[] { match.Groups["absolute"].Value };
            }

            if (match.Groups["range"].Success)
            {
                values = match.Groups["range"].Value.Split('-');
            }

            if (match.Groups["step"].Success)
            {
                suffix = match.Groups["step"].Value;
                suffixType = CronSuffixType.Step;
            }

            if (match.Groups["last"].Success)
            {
                suffixType = CronSuffixType.Last;
            }

            if (match.Groups["week"].Success)
            {
                suffixType = CronSuffixType.Week;
            }

            if (match.Groups["ordinal"].Success)
            {
                suffix = match.Groups["ordinal"].Value;
                suffixType = CronSuffixType.Ordinal;
            }

            return Parse(values, suffix, suffixType);
        }
    }

    namespace Subexpressions
    {
        public class CronSecond : CronSubexpression
        {
            public CronSecond(string expression) : base(expression, 0, 59) { }

            protected override bool IsMatch(DateTime dateTime, int? suffix, CronSuffixType suffixType)
            {
                return suffixType == CronSuffixType.Step && (dateTime.Second - Min) % suffix.Value == 0;
            }
        }

        public class CronMinute : CronSubexpression
        {
            public CronMinute(string expression) : base(expression, 0, 59) { }

            protected override bool IsMatch(DateTime dateTime, int? suffix, CronSuffixType suffixType)
            {
                return suffixType == CronSuffixType.Step && (dateTime.Minute - Min) % suffix.Value == 0;
            }
        }

        public class CronHour : CronSubexpression
        {
            public CronHour(string expression) : base(expression, 0, 23) { }

            protected override bool IsMatch(DateTime dateTime, int? suffix, CronSuffixType suffixType)
            {
                return suffixType == CronSuffixType.Step && (dateTime.Hour - Min) % suffix.Value == 0;
            }
        }

        public class CronDayOfMonth : CronSubexpression
        {
            public CronDayOfMonth(string expression) : base(expression, 1, 31) { }

            protected override bool IsMatch(DateTime dateTime, int? suffix, CronSuffixType suffixType)
            {
                return suffixType == CronSuffixType.Step && (dateTime.Day - Min) % suffix.Value == 0;
            }
        }

        public class CronMonth : CronSubexpression
        {
            private enum MonthName { Jan = 0, Feb }

            public CronMonth(string expression) : base(expression, 0, 6, typeof(MonthName)) { }

            protected override bool IsMatch(DateTime dateTime, int? suffix, CronSuffixType suffixType)
            {
                return suffixType == CronSuffixType.Step && (dateTime.Month - 1 - Min) % suffix.Value == 0;
            }
        }

        public class CronDayOfWeek : CronSubexpression
        {
            private enum DayName { Sun = 1, Mon, Tue, Wed, Thu, Fri, Sat }

            public CronDayOfWeek(string expression) : base(expression, 0, 6, typeof(DayName)) { }

            protected override bool IsMatch(DateTime dateTime, int? suffix, CronSuffixType suffixType)
            {
                return true;
            }
        }

        public class CronYear : CronSubexpression
        {
            public CronYear(string expression) : base(expression, 1970, 2099) { }

            protected override bool IsMatch(DateTime dateTime, int? suffix, CronSuffixType suffixType)
            {
                return true;
            }
        }

    }
}
