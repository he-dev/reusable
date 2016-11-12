using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Collections
{
    public class FibonacciSequence<T> : GeneratedSequence<T>
    {
        private T _preview;
        private T _current;
        private readonly Func<T, T, T> _sum;

        public FibonacciSequence(T firstTwo, T firstStep, int count, Func<T, T, T> sum) : base(count)
        {
            _sum = sum;
            _preview = firstTwo;
            _current = _sum(_preview, firstStep);
        }

        protected override IEnumerable<T> Generate()
        {
            yield return _preview;
            yield return _preview;
            yield return _current;

            while (true)
            {
                var newCurrent = _sum(_preview, _current);
                yield return newCurrent;
                _preview = _current;
                _current = newCurrent;
            }
        }
    }
}
