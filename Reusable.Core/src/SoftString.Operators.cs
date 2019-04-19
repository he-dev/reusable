using System.Diagnostics;

namespace Reusable
{
    public partial class SoftString 
    {
        public static explicit operator string(SoftString obj) => obj?._value;

        [DebuggerStepThrough]
        public static implicit operator SoftString(string value) => value == null ? default : new SoftString(value);

        [DebuggerStepThrough]
        public static implicit operator bool(SoftString value) => !IsNullOrEmpty(value);

        public static bool operator ==(SoftString left, SoftString right) => Comparer.Equals(left, right);

        public static bool operator !=(SoftString left, SoftString right) => !(left == right);

        public static bool operator ==(SoftString left, string right) => Comparer.Equals(left, right);

        public static bool operator !=(SoftString left, string right) => !(left == right);

        public static bool operator ==(string left, SoftString right) => Comparer.Equals(left, right);

        public static bool operator !=(string left, SoftString right) => !(left == right);
    }
}
