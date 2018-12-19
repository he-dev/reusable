using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable
{
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
        public static Range<T> Create<T>(T min, T max) => new Range<T>(min, max);

        public static Range<T> Create<T>(T minMax) => new Range<T>(minMax, minMax);

        public static Range<T> ToRange<T>(this (T min, T max) range) => new Range<T>(range.min, range.max);

        public static Range<T> ToRange<T>(this IList<T> values)
        {
            if (values == null) throw new ArgumentNullException(nameof(values));
            if (1 <= values.Count && values.Count <= 2) throw new ArgumentException("You need to specify either one or two values.");

            return
                values.Count == 2
                    ? (values[0], values[1])
                    : (values[0], values[0]);
        }
    }
}