using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable
{
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
            if (BinaryOperation<T>.LessThanOrEqual(min, max))
            {
                return;
            }

            throw new ArgumentException($"{nameof(Min)} must be <= {nameof(Max)}.");
        }

        public void Deconstruct(out T min, out T max)
        {
            min = Min;
            max = Max;
        }

        public static implicit operator Range<T>((T min, T max) range) => new Range<T>(range.min, range.max);
    }

    public static class Range
    {
        [NotNull]
        public static Range<T> Create<T>(T min, T max) => new Range<T>(min, max);

        [NotNull]
        public static Range<T> Create<T>(T minMax) => new Range<T>(minMax, minMax);

        [NotNull]
        public static Range<T> ToRange<T>(this (T min, T max) range) => new Range<T>(range.min, range.max);

        [NotNull]
        public static Range<T> ToRange<T>(this IList<T> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));

            switch (values.Count)
            {
                case 1: return (values[0], values[0]);
                case 2: return (values[0], values[1]);
                default: throw new ArgumentException("Range can be created only from either one or two values.");
            }
        }
    }
}