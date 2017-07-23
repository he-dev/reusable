using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable
{
    public class CaseInsensitiveString : IEquatable<CaseInsensitiveString>, IEquatable<string>, IComparable<CaseInsensitiveString>, IComparable<string>, IEnumerable<char>
    {
        private static readonly IEqualityComparer<string> Comparer = StringComparer.OrdinalIgnoreCase;

        private readonly string value;

        public CaseInsensitiveString() { }

        public CaseInsensitiveString(string value)
        {
            this.value = value;
        }

        public static readonly CaseInsensitiveString Empty = new CaseInsensitiveString(string.Empty);

        public char this[int index] => value[index];

        public int Length => value.Length;

        public static CaseInsensitiveString Create(string value) => new CaseInsensitiveString(value);

        public bool StartsWith(string value) => this.value.StartsWith(value, StringComparison.OrdinalIgnoreCase);

        public bool EndsWith(string value) => this.value.EndsWith(value, StringComparison.OrdinalIgnoreCase);
        
        public bool IsMatch([NotNull, RegexPattern] string regexp)
        {
            if (regexp == null) throw new ArgumentNullException(nameof(regexp));

            return Regex.IsMatch(value, regexp, RegexOptions.IgnoreCase);
        }

        public override string ToString()
        {
            return value;
        }

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(value);
        }

        #region IEquatable

        public override bool Equals(object obj)
        {
            return
                (obj is CaseInsensitiveString cis && Equals(cis)) ||
                (obj is string s && Equals(s));
        }

        public bool Equals(CaseInsensitiveString other)
        {
            return Equals(other.value);
        }

        public bool Equals(string other)
        {
            return Comparer.Equals(value, other);
        }

        #endregion

        #region IComparable

        public int CompareTo(CaseInsensitiveString other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return CompareTo(other.value);
        }

        public int CompareTo(string other)
        {
            if (ReferenceEquals(value, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return string.Compare(value, other, StringComparison.OrdinalIgnoreCase);
        }
        
        #endregion

        #region IEnumerable

        public IEnumerator<char> GetEnumerator() => value.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        #endregion

        public static bool IsNullOrEmpty(CaseInsensitiveString value) => value == null || string.IsNullOrEmpty(value.value);

        public static bool IsNullOrWhiteSpace(CaseInsensitiveString value) => value == null || string.IsNullOrWhiteSpace(value.value);

        public static explicit operator string(CaseInsensitiveString obj) => obj?.value;

        public static implicit operator CaseInsensitiveString(string value) => new CaseInsensitiveString(value);

        public static bool operator ==(CaseInsensitiveString left, CaseInsensitiveString right) => Comparer.Equals(left?.value, right?.value);

        public static bool operator !=(CaseInsensitiveString left, CaseInsensitiveString right) => !(left == right);

        public static bool operator ==(CaseInsensitiveString left, string right) => Comparer.Equals(left?.value, right);

        public static bool operator !=(CaseInsensitiveString left, string right) => !(left == right);

        public static bool operator ==(string left, CaseInsensitiveString right) => Comparer.Equals(left, right?.value);

        public static bool operator !=(string left, CaseInsensitiveString right) => !(left == right);
    }
}
