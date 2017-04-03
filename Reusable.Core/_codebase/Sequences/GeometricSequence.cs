using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Sequences
{
    // In mathematics, a geometric progression, also known as a geometric sequence, 
    // is a sequence of numbers where each term after the first is found by multiplying the previous one by a fixed, non-zero number called the common ratio.
    public class GeometricSequence<T> : Sequence<T>
    {
        private readonly T _first;
        private readonly double _ratio;
        private readonly Func<T, double, T> _multiply;

        public GeometricSequence(T first, double ratio, Func<T, double, T> multiply)
        {
            _first = first;
            _ratio = ratio;
            _multiply = multiply ?? throw new ArgumentNullException(nameof(multiply));
        }

        protected override IEnumerable<T> Generate()
        {
            var current = _first;
            yield return current;

            while (true)
            {
                yield return (current = _multiply(current, _ratio));
            }
        }
    }

    public class GeometricSequence
    {
        public static IEnumerable<TimeSpan> Double(TimeSpan first, int count)
        {
            return new GeometricSequence<TimeSpan>(first, 2, (x, y) => TimeSpan.FromTicks((int)(x.Ticks * y))).Take(count);
        }

        public static IEnumerable<TimeSpan> Triple(TimeSpan first, int count)
        {
            return new GeometricSequence<TimeSpan>(first, 3, (x, y) => TimeSpan.FromTicks((int)(x.Ticks * y))).Take(count);
        }

        public static IEnumerable<TimeSpan> Halve(TimeSpan first, int count)
        {
            return new GeometricSequence<TimeSpan>(first, 0.5, (x, y) => TimeSpan.FromTicks((int)(x.Ticks * y))).Take(count);
        }
    }
}
