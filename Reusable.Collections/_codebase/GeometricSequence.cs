using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Collections
{
    // In mathematics, a geometric progression, also known as a geometric sequence, 
    // is a sequence of numbers where each term after the first is found by multiplying the previous one by a fixed, non-zero number called the common ratio.
    public class GeometricSequence<T> : GeneratedSequence<T>
    {
        private readonly T _first;
        private readonly double _ratio;
        private readonly Func<T, double, T> _multiply;

        public GeometricSequence(int count, T first, double ratio, Func<T, double, T> multiply) : base(count)
        {
            _first = first;
            _ratio = ratio;
            _multiply = multiply;
        }

        protected override IEnumerable<T> Generate()
        {
            var current = _first;
            yield return current;

            while (true)
            {
                yield return (current = _multiply(current, _ratio));
            };
        }
    }
}
