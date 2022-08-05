using System;
using Reusable.Marbles.Collections;

namespace Reusable.Marbles;

public partial class Range<T> : IEquatable<Range<T>>
{
    public bool Equals(Range<T>? other) => AutoEquality<Range<T>>.Comparer.Equals(this, other);

    public override bool Equals(object? obj) => Equals(obj as Range<T>);

    public override int GetHashCode() => AutoEquality<Range<T>>.Comparer.GetHashCode(this);
}