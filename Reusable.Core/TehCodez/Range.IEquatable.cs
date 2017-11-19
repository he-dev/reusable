using System;
using Reusable.Collections;

namespace Reusable
{
    public partial class Range<T> : IEquatable<Range<T>>
    {
        public bool Equals(Range<T> other) => AutoEquality<Range<T>>.Comparer.Equals(this, other);

        public override bool Equals(object obj) => Equals(obj as Range<T>);

        public override int GetHashCode() => AutoEquality<Range<T>>.Comparer.GetHashCode(this);
    }
}
