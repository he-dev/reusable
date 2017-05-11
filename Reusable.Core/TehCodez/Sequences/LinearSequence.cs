using System;
using System.Collections.Generic;

namespace Reusable.Sequences
{
    public class LinearSequence<T> : Sequence<T>
    {
        private readonly T _first;
        private readonly T _constant;
        private readonly Func<T, T, T> _add;

        public LinearSequence(T first, T constant, Func<T, T, T> add)
        {
            _first = first;
            _constant = constant;
            _add = add ?? throw new ArgumentNullException(nameof(add));
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
