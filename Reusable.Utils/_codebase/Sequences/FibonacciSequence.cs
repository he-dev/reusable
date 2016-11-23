using System;
using System.Collections.Generic;

namespace Reusable.Sequences
{
    public class FibonacciSequence<T> : GeneratedSequence<T>
    {
        private readonly T _first;
        private readonly Func<T, T, T> _sum;

        public FibonacciSequence(int count, T first, Func<T, T, T> sum) : base(count)
        {
            _first = first;
            _sum = sum;
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
}
