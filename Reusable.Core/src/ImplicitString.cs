using System;
using System.Text.RegularExpressions;
using Reusable.Collections;

namespace Reusable
{
    public class ImplicitString : IEquatable<ImplicitString>
    {
        public ImplicitString(string value) => Value = value;

        [AutoEqualityProperty]
        public string Value { get; }

        public override string ToString() => Value;

        public static implicit operator ImplicitString(string value) => new ImplicitString(value);

        public static implicit operator ImplicitString(Group group) => group.Value;

        public static implicit operator string(ImplicitString value) => value.ToString();

        public static implicit operator bool(ImplicitString value) => !string.IsNullOrWhiteSpace(value);

        #region IEquatable

        public bool Equals(ImplicitString other) => AutoEquality<ImplicitString>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => obj is ImplicitString str && Equals(str);

        public override int GetHashCode() => AutoEquality<ImplicitString>.Comparer.GetHashCode(this);

        #endregion
    }
}