using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Collections
{
    public class LinearSequence<T> : GeneratedSequence<T>
    {
        private readonly T _first;
        private readonly T _constant;
        private readonly Func<T, T, T> _add;

        public LinearSequence(int count, T first, T constant, Func<T, T, T> add) : base(count)
        {
            _first = first;
            _constant = constant;
            _add = add;
        }

        protected override IEnumerable<T> Generate()
        {
            var current = _first;
            yield return current;

            while (true)
            {
                yield return (current = _add(current, _constant));
            };
        }
    }
}
