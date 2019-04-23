using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable
{
    [Serializable]
    public partial class SoftString : IEnumerable<char>
    {
        public static readonly SoftStringComparer Comparer = new SoftStringComparer();

        private readonly string _value;

        [DebuggerStepThrough]
        public SoftString([NotNull] string value)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            _value = value.Trim();
        }

        public SoftString() : this(string.Empty)
        {
        }

        [DebuggerStepThrough]
        public SoftString(char value)
            : this(value.ToString())
        {
        }

        [NotNull]
        public static SoftString Empty { get; } = new SoftString();

        public char this[int index] => _value[index];

        public int Length => _value.Length;

        [NotNull]
        public static SoftString Create([NotNull] string value) => new SoftString(value);

        public bool StartsWith(string value) => _value.StartsWith((string)(SoftString)value, StringComparison.OrdinalIgnoreCase);

        public bool EndsWith(string value) => _value.EndsWith((string)(SoftString)value, StringComparison.OrdinalIgnoreCase);

        public bool IsMatch([NotNull, RegexPattern] string pattern, RegexOptions options = RegexOptions.None)
        {
            if (pattern == null) throw new ArgumentNullException(nameof(pattern));

            return Regex.IsMatch(ToString(), pattern, options | RegexOptions.IgnoreCase);
        }

        public override string ToString() => _value;

        #region IEnumerable

        public IEnumerator<char> GetEnumerator() => ToString().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        public static bool IsNullOrEmpty([CanBeNull] SoftString value) => string.IsNullOrEmpty(value?._value);

        public static bool IsNullOrWhiteSpace([CanBeNull] SoftString value) => string.IsNullOrWhiteSpace(value?._value);
    }
}