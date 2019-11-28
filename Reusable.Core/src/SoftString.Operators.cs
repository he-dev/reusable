using System.Diagnostics;
using JetBrains.Annotations;

namespace Reusable
{
    public partial class SoftString
    {
        [ContractAnnotation("obj: null => null; notnull => notnull")]
        public static explicit operator string?(SoftString? obj) => obj?._value;

        [DebuggerStepThrough]
        [ContractAnnotation("value: null => null; notnull => notnull")]
        public static implicit operator SoftString(string value) => value is {} ? new SoftString(value) : default;

        [DebuggerStepThrough]
        public static implicit operator bool(SoftString? value) => !IsNullOrEmpty(value);

        public static bool operator ==(SoftString? left, SoftString? right) => Comparer.Equals(left, right);

        public static bool operator !=(SoftString? left, SoftString? right) => !(left == right);

        public static bool operator ==(SoftString? left, string? right) => Comparer.Equals(left, right);

        public static bool operator !=(SoftString? left, string? right) => !(left == right);

        public static bool operator ==(string? left, SoftString? right) => Comparer.Equals(left, right);

        public static bool operator !=(string? left, SoftString? right) => !(left == right);
    }
}