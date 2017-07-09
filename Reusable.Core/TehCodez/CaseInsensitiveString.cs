using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable
{
    public class CaseInsensitiveString : IEquatable<CaseInsensitiveString>, IEquatable<string>
    {
        private static readonly IEqualityComparer<string> Comparer = StringComparer.OrdinalIgnoreCase;

        private readonly string value;

        public CaseInsensitiveString() { }

        public CaseInsensitiveString(string value)
        {
            this.value = value;
        }

        public static CaseInsensitiveString Empty => new CaseInsensitiveString(string.Empty);

        public override int GetHashCode()
        {
            return Comparer.GetHashCode(value);
        }

        public override bool Equals(object obj)
        {
            return
                (obj is CaseInsensitiveString cis && Equals(cis)) ||
                (obj is string s && Equals(s));
        }

        public bool Equals(CaseInsensitiveString other)
        {
            return Equals(other?.value);
        }

        public bool Equals(string other)
        {
            return Comparer.Equals(value, other);
        }

        public override string ToString()
        {
            return value;
        }

        public static explicit operator string(CaseInsensitiveString obj) => obj.value;

        public static implicit operator CaseInsensitiveString(string value) => new CaseInsensitiveString(value);

        public static bool operator ==(CaseInsensitiveString left, CaseInsensitiveString right) => Comparer.Equals(left?.value, right?.value);

        public static bool operator !=(CaseInsensitiveString left, CaseInsensitiveString right) => !(left == right);

        public static bool operator ==(CaseInsensitiveString left, string right) => Comparer.Equals(left?.value, right);

        public static bool operator !=(CaseInsensitiveString left, string right) => !(left == right);

        public static bool operator ==(string left, CaseInsensitiveString right) => Comparer.Equals(left, right?.value);

        public static bool operator !=(string left, CaseInsensitiveString right) => !(left == right);

    }
}
