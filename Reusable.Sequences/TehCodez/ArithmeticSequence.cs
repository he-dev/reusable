using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Reusable.Sequences
{
    public class ArithmeticSequence<T> : Sequence<T>
    {
        private readonly T _first;
        private readonly T _step;        

        public ArithmeticSequence(T first, T step)
        {
            _first = first;
            _step = step;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            var current = _first;

            while (true)
            {
                yield return current;
                current = BinaryOperation<T>.Add(current, _step);
            }
            // ReSharper disable once IteratorNeverReturns
            // by design, user must use Take()
        }
    }
}
