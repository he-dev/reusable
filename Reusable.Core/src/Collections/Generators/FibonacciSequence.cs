using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Reusable.Collections.Generators
{
    public class FibonacciSequence<T> : Sequence<T>
    {
        private readonly T _one;

        public FibonacciSequence(T one)
        {
            _one = one;            
        }

        public override IEnumerator<T> GetEnumerator()
        {
            yield return _one;

            var previous = _one;
            var current = _one;

            while (true)
            {
                yield return current;

                var newCurrent = BinaryOperation<T>.Add(previous, current);
                previous = current;
                current = newCurrent;
            }
            // ReSharper disable once IteratorNeverReturns - this is by design
        }
    }    
}
