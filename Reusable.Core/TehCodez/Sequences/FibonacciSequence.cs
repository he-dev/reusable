using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Sequences
{
    public class FibonacciSequence<T> : Sequence<T>
    {
        private readonly T _first;
        private readonly Func<T, T, T> _sum;

        public FibonacciSequence(T first, Func<T, T, T> sum)
        {
            _first = first;
            _sum = sum ?? throw new ArgumentNullException(nameof(sum));
        }
       
        protected override IEnumerable<T> Generate()
        {
            yield return _first;
            yield return _first;

            var preview = _first;
            var current = _sum(_first, _first);

            yield return current;

            while (true)
            {
                var newCurrent = _sum(preview, current);
                yield return newCurrent;
                preview = current;
                current = newCurrent;
            }
        }
    }

    public class FibonacciSequence
    {
        public static IEnumerable<TimeSpan> Create(TimeSpan first, int count)
        {
            return new FibonacciSequence<TimeSpan>(first, (x, y) => x + y).Take(count);
        }

        public static IEnumerable<int> Create(int first, int count)
        {
            return new FibonacciSequence<int>(first, (x, y) => x + y).Take(count);
        }

        public static IEnumerable<int> Create(int count)
        {
            return new FibonacciSequence<int>(1, (x, y) => x + y).Take(count);
        }
    }
}
