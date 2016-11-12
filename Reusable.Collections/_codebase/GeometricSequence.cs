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
        private T _current;
        private readonly Func<T, T> _multiply;
        public GeometricSequence(T first, Func<T, T> multiply, int count) : base(count)
        {
            _current = first;
            _multiply = multiply;
        }
        protected override IEnumerable<T> Generate()
        {
            yield return _current;

            while (true)
            {
                yield return (_current = _multiply(_current));
            };
        }
    }
}
