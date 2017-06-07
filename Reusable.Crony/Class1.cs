using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Reusable.Crony
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

    public enum CronField
    {
        Seconds,
        Minutes,
        Hours,
        DayOfMonth,
        Month,
        DayOfWeek,
        Year
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class CronRange
    {
        private static readonly IReadOnlyDictionary<string, int> DaysOfWeek = new[]
        {
            "SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"
        }
        .Select((dayOfWeek, index) => new { dayOfWeek, index })
        .ToDictionary(x => x.dayOfWeek, x => x.index + 1, StringComparer.OrdinalIgnoreCase);

        private static readonly IReadOnlyDictionary<string, int> Months = new[]
        {
            "JAN", "FEB", "MAR", "APR", "MAI", "JUN", "JUL", "AUG", "SEP", "OCT", "NOV", "DEC"
        }
        .Select((month, index) => new { month, index })
        .ToDictionary(x => x.month, x => x.index + 1, StringComparer.OrdinalIgnoreCase);

        public CronRange(int? min, int? max, int? step, CronExtension extension)
        {
            Min = min;
            Max = max ?? min;
            Step = step;
            Extension = extension;
            if (min.HasValue && max.HasValue && min > max) { throw new ArgumentException("min must be less then max."); }
            if (step.HasValue && step < 1) { throw new ArgumentException("step must be positive."); }
        }

        private static CronRange Empty => new CronRange(null, null, null, CronExtension.None);

        public static IEqualityComparer<CronRange> Comparer { get; } = new CronRangeEqualityComparer();

        public bool IsEmpty => !Min.HasValue;

        public int? Min { get; }

        public int? Max { get; }

        public int? Step { get; }

        public CronExtension Extension { get; }

        private const string Pattern = @"(?<Min>{0})(?:-(?<Max>{0}))?(?:\/(?<Step>{0}))?((?:#(?<nth>[1-5]))|(?<Ext>L|W))?";

        public static CronRange From(string input)
        {
            if (string.IsNullOrEmpty(input)) { throw new ArgumentNullException(nameof(input)); }

            input =
                input
                    .NormalizeCronString()
                    .CleanUpCronString();

            if (input == "*") { return Empty; }

            return
                FromNumeric(input) ??
                FromLiteral(input) ??
                throw new ArgumentException(nameof(input));
        }

        private static CronRange FromNumeric(string input)
        {
            var match = Regex.Match(input, string.Format(Pattern, @"\d+"), RegexOptions.ExplicitCapture);

            if (!match.Success) { return null; }

            var range = Empty;

            if (int.TryParse(match.Groups["Min"].Value, out var min)) range = range.WithMin(min);
            if (int.TryParse(match.Groups["Max"].Value, out var max)) range = range.WithMax(max);
            if (int.TryParse(match.Groups["Step"].Value, out var step)) range = range.WithStep(step);
            if (int.TryParse(match.Groups["nth"].Value, out var nth)) range = range.WithExtension((CronExtension)nth);
            if (Enum.TryParse<CronExtension>(match.Groups["Ext"].Value, out var ext)) range = range.WithExtension(ext);

            return range;
        }

        private static CronRange FromLiteral(string input)
        {
            foreach (var literals in new[] { DaysOfWeek, Months })
            {
                var match = Regex.Match(input, string.Format(Pattern, string.Join("|", literals.Keys)));

                if (!match.Success) { continue; }

                var range = Empty;

                if (literals.TryGetValue(match.Groups["Min"].Value, out var min)) range = range.WithMin(min);
                if (literals.TryGetValue(match.Groups["Max"].Value, out var max)) range = range.WithMax(max);
                if (literals.TryGetValue(match.Groups["Step"].Value, out var step)) range = range.WithStep(step);

                return range;
            }
            return null;
        }

        private string DebuggerDisplay => ToString();

        public override string ToString()
        {
            if (IsEmpty) { return "*"; }

            return
                new StringBuilder()
                    .Append(Min)
                    .Append(Max.HasValue && Max > Min ? $"-{Max}" : string.Empty)
                    .Append(Step.HasValue ? $"/{Step}" : string.Empty)
                    .ToString();
        }
    }

    public static class CronRangeBuilder
    {
        public static CronRange WithMin(this CronRange range, int min) => new CronRange(min, range.Max, range.Step, range.Extension);

        public static CronRange WithMax(this CronRange range, int max) => new CronRange(range.Min, max, range.Step, range.Extension);

        public static CronRange WithStep(this CronRange range, int step) => new CronRange(range.Min, range.Max, step, range.Extension);

        public static CronRange WithExtension(this CronRange range, CronExtension extension) => new CronRange(range.Min, range.Max, range.Step, extension);
    }

    public static class CronString
    {
        public static string NormalizeCronString(this string input) => input.Trim().ToUpper();
        public static string CleanUpCronString(this string input) => Regex.Replace(input, @"\s+", " ");
    }

    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public abstract class CronSubexpression : List<CronRange>, IGrouping<CronField, CronRange>
    {
        /// <remarks>Duplicate entries are ignored.</remarks>
        protected CronSubexpression(CronField field, IEnumerable<CronRange> ranges) : base(ranges.Distinct(CronRange.Comparer))
        {
            Key = field;
            if (ranges == null) { throw new ArgumentNullException(nameof(ranges)); }
            if (this.Count(r => r.IsEmpty) > 1) { throw new ArgumentException(paramName: nameof(ranges), message: $"{Key} can have only one empty range."); }
            if (this.Any(r => r.IsEmpty) && this.Count() > 1) { throw new ArgumentException(paramName: nameof(ranges), message: $"{Key} must not have other ranges if there is an empty one."); }
            if (this.Any(r => r.Min < Min || r.Min > Max)) { throw new ArgumentOutOfRangeException(paramName: nameof(ranges), message: $"{Key} must be between {Min}-{Max}."); }
            if (this.Any(r => r.Max < Min || r.Max > Max)) { throw new ArgumentOutOfRangeException(paramName: nameof(ranges), message: $"{Key} must be between {Min}-{Max}."); }
        }

        public CronField Key { get; }

        public abstract int Min { get; }

        public abstract int Max { get; }

        private string DebuggerDisplay => ToString();

        public bool Contains(DateTime timestamp)
        {
            if (this.Any(x => x.IsEmpty))
            {
                return true;
            }

            var value = GetDatePart(timestamp);

            foreach (var range in this)
            {
                if (range.Min <= value && value <= range.Max)
                {
                    if (range.Step.HasValue)
                    {
                        var step = range.Step.Value;
                        while (step <= value)
                        {
                            if (value == step) { return true; }
                            step += range.Min.Value;
                        }
                        return false;
                    }
                    return true;
                }
            }

            return false;
        }

        protected abstract int GetDatePart(DateTime timestamp);

        public override string ToString() => string.Join(",", this.Select(se => se.ToString()));
    }

    public class CronSecond : CronSubexpression
    {
        public CronSecond(IEnumerable<CronRange> ranges) : base(CronField.Seconds, ranges) { }

        public override int Min => 0;

        public override int Max => 59;

        protected override int GetDatePart(DateTime timestamp) => timestamp.Second;
    }

    public class CronMinute : CronSubexpression
    {
        public CronMinute(IEnumerable<CronRange> ranges) : base(CronField.Minutes, ranges) { }

        public override int Min => 0;

        public override int Max => 59;

        protected override int GetDatePart(DateTime timestamp) => timestamp.Minute;
    }

    public class CronHour : CronSubexpression
    {
        public CronHour(IEnumerable<CronRange> ranges) : base(CronField.Hours, ranges) { }

        public override int Min => 0;

        public override int Max => 23;

        protected override int GetDatePart(DateTime timestamp) => timestamp.Hour;
    }

    public class CronDayOfMonth : CronSubexpression
    {
        public CronDayOfMonth(IEnumerable<CronRange> ranges) : base(CronField.DayOfMonth, ranges) { }

        public override int Min => 1;

        public override int Max => 31;

        protected override int GetDatePart(DateTime timestamp) => timestamp.Day;
    }

    public class CronMonth : CronSubexpression
    {
        public CronMonth(IEnumerable<CronRange> ranges) : base(CronField.Month, ranges) { }

        public override int Min => 0;

        public override int Max => 11;

        protected override int GetDatePart(DateTime timestamp) => timestamp.Month;
    }

    public class CronDayOfWeek : CronSubexpression
    {
        public CronDayOfWeek(IEnumerable<CronRange> ranges) : base(CronField.DayOfWeek, ranges) { }

        public override int Min => 1;

        public override int Max => 7;

        protected override int GetDatePart(DateTime timestamp) => (int)timestamp.DayOfWeek;
    }

    public class CronYear : CronSubexpression
    {
        public CronYear(IEnumerable<CronRange> ranges) : base(CronField.Year, ranges) { }

        public override int Min => DateTime.MinValue.Year;

        public override int Max => DateTime.MaxValue.Year;

        protected override int GetDatePart(DateTime timestamp) => timestamp.Year;
    }

    internal delegate CronSubexpression CreateSubExpressionCallback(IEnumerable<CronRange> ranges);

    public class CronExpression : IEnumerable<CronSubexpression>
    {
        private static readonly IEnumerable<CreateSubExpressionCallback> SubExpressionFactories = new CreateSubExpressionCallback[]
        {
            ranges => new CronSecond(ranges),
            ranges => new CronMinute(ranges),
            ranges => new CronHour(ranges),
            ranges => new CronDayOfMonth(ranges),
            ranges => new CronMonth(ranges),
            ranges => new CronDayOfWeek(ranges),
            ranges => new CronYear(ranges),
        };

        private readonly List<CronSubexpression> _subexpressions;

        private CronExpression(IEnumerable<CronSubexpression> subExpressions)
        {
            _subexpressions = subExpressions.ToList();
        }

        public static CronExpression From(string input)
        {
            if (string.IsNullOrEmpty(input)) { throw new ArgumentNullException(nameof(input)); }

            var subExpressions =
                Regex
                    .Split(input, @"\s")
                    .Where(f => !string.IsNullOrEmpty(f))
                    .Zip(SubExpressionFactories, (field, factory) => new { field, factory })
                    .Select(x => x.factory(Regex.Split(x.field, @",").Select(CronRange.From)));

            return new CronExpression(subExpressions);
        }

        public bool Contains(DateTime timestamp) => this.All(x => x.Contains(timestamp));

        public IEnumerator<CronSubexpression> GetEnumerator() => _subexpressions.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public override string ToString() => string.Join(" ", this.Select(x => x.ToString()));
    }

    public class CronRangeEqualityComparer : IEqualityComparer<CronRange>
    {
        public bool Equals(CronRange x, CronRange y)
        {
            return
                !ReferenceEquals(x, null) &&
                !ReferenceEquals(y, null) &&
                x.Min == y.Min &&
                x.Max == y.Max &&
                x.Step == y.Step;
        }

        public int GetHashCode(CronRange obj)
        {
            return new[]
            {
                obj.Min.GetHashCode(),
                obj.Max.GetHashCode(),
                obj.Step.GetHashCode()
            }
            .Aggregate(0, (current, next) => (current * 397) ^ next);
        }
    }

    public class CronSubexpressionEqualityComparer : IEqualityComparer<CronSubexpression>
    {
        public bool Equals(CronSubexpression x, CronSubexpression y)
        {
            return
                !ReferenceEquals(x, null) &&
                !ReferenceEquals(y, null) &&
                x.SequenceEqual(y, CronRange.Comparer);
        }

        public int GetHashCode(CronSubexpression obj)
        {
            return obj.Aggregate(0, (current, next) => (current * 397) ^ next.GetHashCode());
        }
    }

    public class CronScheduler
    {
        
    }
}
