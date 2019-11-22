using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable
{
    /// <summary>
    /// This special string class represents a string that is trimmed and implements case-insensitive comparison.
    /// </summary>
    [Serializable]
    public partial class SoftString : IEnumerable<char>
    {
        public static readonly SoftStringComparer Comparer = new SoftStringComparer();

        private readonly string _value;

        [DebuggerStepThrough]
        public SoftString(string value) => _value = value.Trim();

        public SoftString() : this(string.Empty) { }

        [DebuggerStepThrough]
        public SoftString(char value)
            : this(value.ToString()) { }

        [NotNull]
        public static SoftString Empty { get; } = new SoftString();

        public char this[int index] => _value[index];

        public int Length => _value.Length;

        [NotNull]
        public static SoftString Create(string value) => new SoftString(value);

        public bool StartsWith(string value) => _value.StartsWith((string)(SoftString)value, StringComparison.OrdinalIgnoreCase);

        public bool EndsWith(string value) => _value.EndsWith((string)(SoftString)value, StringComparison.OrdinalIgnoreCase);

        public bool IsMatch([RegexPattern] string pattern, RegexOptions options = RegexOptions.None)
        {
            return Regex.IsMatch(ToString(), pattern, options | RegexOptions.IgnoreCase);
        }

        public override string ToString() => _value;

        #region IEnumerable

        public IEnumerator<char> GetEnumerator() => ToString().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        public static bool IsNullOrEmpty(SoftString? value) => string.IsNullOrEmpty(value?._value);

        public static bool IsNullOrWhiteSpace(SoftString? value) => string.IsNullOrWhiteSpace(value?._value);
    }
}