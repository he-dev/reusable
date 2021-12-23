using System;
using System.Collections.Generic;
using Reusable.Essentials.Collections;

namespace Reusable.Essentials;

[Serializable]
public partial class Range<T>
{
    public Range(T min, T max)
    {
        ValidateMinLessThanOrEqualMax(min, max);

        Min = min;
        Max = max;
    }

    [AutoEqualityProperty]
    public T Min { get; }

    [AutoEqualityProperty]
    public T Max { get; }

    private void ValidateMinLessThanOrEqualMax(T min, T max)
    {
        if (!BinaryOperation<T>.LessThanOrEqual(min, max))
        {
            throw new ArgumentException($"{nameof(Min)} must be <= {nameof(Max)}.");
        }
    }

    public void Deconstruct(out T min, out T max)
    {
        min = Min;
        max = Max;
    }

    public static implicit operator Range<T>((T min, T max) range) => new(range.min, range.max);
}

public static class Range
{
    public static Range<T> Create<T>(T min, T max) => new(min, max);

    public static Range<T> Create<T>(T minMax) => new(minMax, minMax);

    public static Range<T> ToRange<T>(this (T min, T max) range) => new(range.min, range.max);

    public static Range<T> ToRange<T>(this IList<T> values)
    {
        return values.Count switch
        {
            1 => (values[0], values[0]),
            2 => (values[0], values[1]),
            _ => throw new ArgumentException("Range can be created only from either one or two values.")
        };
    }
}