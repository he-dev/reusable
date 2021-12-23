using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Essentials.Collections;

namespace Reusable.Essentials;

/// <summary>
/// Decorator for making uncomparable types comparable in a nicer way.
/// </summary>
/// <typeparam name="T"></typeparam>
[PublicAPI]
public class Comparable<T> : IComparable<T>
{
    private readonly IComparer<T> _comparer;

    public Comparable(T value, IComparer<T> comparer)
    {
        Value = value;
        _comparer = comparer;
    }

    [AutoEqualityProperty]
    public T Value { get; }

    public int CompareTo(T? other) => _comparer.Compare(Value, other);

    public static implicit operator T(Comparable<T> comparable) => comparable.Value;

    public static bool operator <(Comparable<T> left, Comparable<T> right) => left.CompareTo(right.Value) < 0;

    public static bool operator >(Comparable<T> left, Comparable<T> right) => left.CompareTo(right.Value) > 0;

    public static bool operator <=(Comparable<T> left, Comparable<T> right) => left.CompareTo(right.Value) <= 0;

    public static bool operator >=(Comparable<T> left, Comparable<T> right) => left.CompareTo(right.Value) >= 0;

    public static bool operator ==(Comparable<T> left, Comparable<T> right) => AutoEquality<Comparable<T>>.Comparer.Equals(left, right);

    public static bool operator !=(Comparable<T> left, Comparable<T> right) => !(left == right);

    protected bool Equals(Comparable<T> other) => AutoEquality<Comparable<T>>.Comparer.Equals(this, other);

    public override bool Equals(object? obj) => obj is Comparable<T> comparable && Equals(comparable);

    public override int GetHashCode() => AutoEquality<Comparable<T>>.Comparer.GetHashCode(this);
}