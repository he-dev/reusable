using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Sequences
{
    public class HarmonicSequence<T> : Sequence<T>
    {
        private readonly LinearSequence<T> _linear;
        private readonly T _first;
        private readonly Func<T, T, T> _divide;

        public HarmonicSequence(LinearSequence<T> linear, T first, Func<T, T, T> divide)
        {
            _linear = linear;
            _first = first;
            _divide = divide ?? throw new ArgumentNullException(nameof(divide));
        }

        protected override IEnumerable<T> Generate()
        {
            return _linear.Select(divisor => _divide(_first, divisor));
        }
    }
}
