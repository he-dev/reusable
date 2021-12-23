using System;
using System.Collections.Generic;

namespace Reusable.Essentials.Collections;

internal class LambdaEqualityComparer<T> : IEqualityComparer<T>
{
    private readonly Func<T, T, bool> _equals;
    private readonly Func<T, int> _getHashCode;

    internal LambdaEqualityComparer(Func<T, T, bool> equals, Func<T, int> getHashCode)
    {
        _equals = equals;
        _getHashCode = getHashCode;
    }

    public bool Equals(T x, T y)
    {
        if (ReferenceEquals(null, x)) return false;
        if (ReferenceEquals(null, y)) return false;
        if (ReferenceEquals(x, y)) return true;
        return _equals(x, y);
    }

    public int GetHashCode(T obj) => _getHashCode(obj);
}

public static class EqualityComparer
{
    /// <summary>
    /// Creates an equality-comparer with object's hash-code.
    /// </summary>
    public static IEqualityComparer<T> Create<T>(Func<T, T, bool> equals)
    {
        return Create(equals, obj => obj?.GetHashCode() ?? 0);
    }

    public static IEqualityComparer<T> Create<T>(Func<T, T, bool> equals, Func<T, int> getHashCode)
    {
        return new LambdaEqualityComparer<T>(equals, getHashCode);
    }

    public static IEqualityComparer<TSource> Create<TSource, T>(Func<TSource, T> keySelector, IEqualityComparer<T>? comparer = default)
    {
        comparer ??= EqualityComparer<T>.Default;

        return EqualityComparerFactory<TSource>.Create
        (
            getHashCode: obj => comparer.GetHashCode(keySelector(obj)),
            @equals: (x, y) => comparer.Equals(keySelector(x), keySelector(y))
        );
    }
}
    
//public delegate bool EqualsDelegate<T>([all])

public static class EqualityComparerFactory<T>
{
    /// <summary>
    /// Creates an equality-comparer with object's hash-code.
    /// </summary>
    public static IEqualityComparer<T> Create(Func<T, T, bool> equals)
    {
        return Create(equals, obj => obj?.GetHashCode() ?? 0);
    }

    public static IEqualityComparer<T> Create(Func<T, T, bool> equals, Func<T, int> getHashCode)
    {
        return new LambdaEqualityComparer<T>(equals, getHashCode);
    }
}