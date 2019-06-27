using System;
using System.Collections.Generic;

namespace Reusable.Collections.Generators
{
    public class FibonacciSequence<T> : Sequence<T>
    {
        public FibonacciSequence(T one) : base(Create(one)) { }

        private static IEnumerable<T> Create(T one)
        {
            yield return one;

            var previous = one;
            var current = one;

            foreach (var _ in Sequence.InfiniteDefault<T>())
            {
                yield return current;

                var newCurrent = BinaryOperation<T>.Add(previous, current);
                previous = current;
                current = newCurrent;
            }
        }
    }
}