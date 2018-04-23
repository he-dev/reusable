using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Reusable.Sequences
{
    // In mathematics, a geometric progression, also known as a geometric sequence, 
    // is a sequence of numbers where each term after the first is found by multiplying the previous one by a fixed, non-zero number called the common ratio.
    public class GeometricSequence<T> : Sequence<T>
    {
        private readonly T _first;
        private readonly T _ratio;

        public GeometricSequence(T first, T ratio)
        {
            _first = first;
            _ratio = ratio;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            var current = _first;
            yield return current;

            while (true)
            {
                yield return (current = BinaryOperation<T>.Multiply(current, _ratio));
            }
        }
    }
}
