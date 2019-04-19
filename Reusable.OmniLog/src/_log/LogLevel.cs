using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Custom;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Collections;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    [PublicAPI]
    [DebuggerDisplay("{" + nameof(DebuggerDisplay) + ",nq}")]
    public sealed class LogLevel : IEquatable<LogLevel>, IComparable<LogLevel>, IComparable, ILogLevel
    {
        private static readonly LogLevelComparer Comparer = new LogLevelComparer();

        // These two need to be lazy or otherwise we cannot initialize LogLevels from Logger - it'll lock.

        private static readonly Lazy<IDictionary<SoftString, ILogLevel>> NameMap;

        private static readonly Lazy<IDictionary<int, ILogLevel>> FlagMap;

        static LogLevel()
        {
            // Since the field LogLevels is defined on the Logger we cannot use field initializers for the maps here
            // because they are initialized in the wrong order and an type initialization exception is thrown.

            NameMap = new Lazy<IDictionary<SoftString, ILogLevel>>
            (
                () =>
                    Logger
                        .LogLevels
                        .Select(ll => (Key: ll.Name, Value: ll))
                        .ToDictionary(x => x.Key, x => x.Value)
            );
            FlagMap = new Lazy<IDictionary<int, ILogLevel>>
            (
                () =>
                    Logger
                        .LogLevels
                        .Select(ll => (Key: ll.Flag, Value: ll))
                        .ToDictionary(x => x.Key, x => x.Value)
            );
        }

        private LogLevel(SoftString name, int flag)
        {
            Name = name;
            Flag = flag;
        }

        private string DebuggerDisplay => ToString();

        public SoftString Name { get; }

        [AutoEqualityProperty]
        public int Flag { get; }

        public static readonly LogLevel None = new LogLevel(nameof(None), 0);
        public static readonly LogLevel Trace = new LogLevel(nameof(Trace), 1 << 0);
        public static readonly LogLevel Debug = new LogLevel(nameof(Debug), 1 << 1);
        public static readonly LogLevel Information = new LogLevel(nameof(Information), 1 << 2);
        public static readonly LogLevel Warning = new LogLevel(nameof(Warning), 1 << 3);
        public static readonly LogLevel Error = new LogLevel(nameof(Error), 1 << 4);
        public static readonly LogLevel Fatal = new LogLevel(nameof(Fatal), 1 << 5);
        public static readonly LogLevel All = new LogLevel(nameof(All), Trace | Debug | Information | Warning | Error | Fatal);

        [NotNull, ContractAnnotation("value: null => halt; notnull => notnull")]
        public static LogLevel Parse([NotNull] string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            var logLevelNames = Logger.LogLevels.Select(ll => ll.Name.ToString());
            return
                Regex
                    .Matches(value, $"{logLevelNames.Join("|")}", RegexOptions.IgnoreCase)
                    .Cast<Match>()
                    .Select(m => m.Value)
                    // We can safely read the values form the dictionary 
                    // because the regex already took care of the names so we only have valid ones.
                    .Aggregate(None, (logLevel, name) => logLevel | (LogLevel)NameMap.Value[name]);
        }

        public static LogLevel FromValue(int value)
        {
            return
                Logger
                    .LogLevels
                    .Where(logLevel => logLevel.Contains(value))
                    .Aggregate(None, (logLevel, next) => logLevel | (LogLevel)next);
        }

        public bool Contains(ILogLevel other) => (Flag & other.Flag) == other.Flag;

        public bool Contains(int flags) => (Flag & flags) == flags;

        public override string ToString() => Logger.LogLevels.Where(Contains).Select(ll => ll.Name).Join(", ");

        #region IEquatable

        public bool Equals(LogLevel other) => AutoEquality<LogLevel>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as LogLevel);

        public override int GetHashCode() => AutoEquality<LogLevel>.Comparer.GetHashCode(this);

        #endregion

        public int CompareTo(LogLevel other) => Comparer.Compare(this, other);

        public int CompareTo(object other) => Comparer.Compare(this, other);

        public static implicit operator string(LogLevel logLevel) => logLevel?.ToString() ?? throw new ArgumentNullException(nameof(logLevel));

        public static implicit operator int(LogLevel logLevel) => logLevel?.Flag ?? throw new ArgumentNullException(nameof(logLevel));

        public static implicit operator LogLevel(string value) => Parse(value);

        public static implicit operator LogLevel(int value) => FromValue(value);

        #region Operators

        public static bool operator ==(LogLevel left, LogLevel right) => Comparer.Compare(left, right) == 0;
        public static bool operator !=(LogLevel left, LogLevel right) => !(left == right);

        public static bool operator <(LogLevel left, LogLevel right) => Comparer.Compare(left, right) < 0;
        public static bool operator <=(LogLevel left, LogLevel right) => Comparer.Compare(left, right) <= 0;

        public static bool operator >(LogLevel left, LogLevel right) => Comparer.Compare(left, right) > 0;
        public static bool operator >=(LogLevel left, LogLevel right) => Comparer.Compare(left, right) >= 0;

        public static LogLevel operator |(LogLevel left, LogLevel right) => new LogLevel("Custom", left.Flag | right.Flag);

        #endregion

        private class LogLevelComparer : IComparer<ILogLevel>, IComparer
        {
            public int Compare(ILogLevel left, ILogLevel right)
            {
                if (ReferenceEquals(left, right)) return 0;
                if (ReferenceEquals(left, null)) return 1;
                if (ReferenceEquals(right, null)) return -1;
                return left.Flag - right.Flag;
            }

            public int Compare(object left, object right) => Compare(left as ILogLevel, right as ILogLevel);
        }
    }
}