using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;

namespace Reusable.Marbles.Extensions;

public static class RangeExtensions
{
    private static readonly ConcurrentDictionary<(string Method, Type Type), object> Cache = new();

    public static bool ContainsInclusive<T>(this Range<T> range, T value)
    {
        // min >= x && x <= max
        return ((Func<T, T, T, bool>)Cache.GetOrAdd((nameof(ContainsInclusive), typeof(T)), t => CreateComparerFunc<T>(Expression.GreaterThanOrEqual, Expression.LessThanOrEqual)))(range.Min, range.Max, value);
    }

    public static bool ContainsExclusive<T>(this Range<T> range, T value)
    {
        // min >= x && x <= max
        return ((Func<T, T, T, bool>)Cache.GetOrAdd((nameof(ContainsExclusive), typeof(T)), t => CreateComparerFunc<T>(Expression.GreaterThan, Expression.LessThan)))(range.Min, range.Max, value);
    }

    public static bool OverlapsInclusive<T>(this Range<T> range, Range<T> other)
    {
        // min >= x && x <= max || min >= y && y <= max
        return range.ContainsInclusive(other.Min) || range.ContainsInclusive(other.Max);
    }

    public static bool OverlapsExclusive<T>(this Range<T> range, Range<T> other)
    {
        // min >= x && x <= max || min >= y && y <= max
        return range.ContainsExclusive(other.Min) || range.ContainsExclusive(other.Max);
    }

    private static Func<T, T, T, bool> CreateComparerFunc<T>
    (
        Func<Expression, Expression, Expression> greaterThanOrEqual,
        Func<Expression, Expression, Expression> lessThanOrEqual
    )
    {
        var x = Expression.Parameter(typeof(T), "x");
        var min = Expression.Parameter(typeof(T), "min");
        var max = Expression.Parameter(typeof(T), "max");

        var contains = Expression.AndAlso
        (
            greaterThanOrEqual(x, min),
            lessThanOrEqual(x, max)
        );

        return Expression.Lambda<Func<T, T, T, bool>>(contains, min, max, x).Compile();
    }
}