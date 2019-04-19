using System;

namespace Reusable
{
    public partial class SoftString : IEquatable<SoftString>, IEquatable<string>
    {
        public override int GetHashCode() => Comparer.GetHashCode(this);

        public override bool Equals(object obj) => obj is SoftString softString && Equals(softString);

        public bool Equals(SoftString other) => Comparer.Equals(this, other);

        public bool Equals(string other) => Equals((SoftString)other);
    }
}
