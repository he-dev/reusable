using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Collections
{
    public class HarmonicSequence<T> : GeneratedSequence<T>
    {
        private readonly LinearSequence<T> _linear;
        private readonly T _first;
        private readonly Func<T, T, T> _divide;

        public HarmonicSequence(LinearSequence<T> linear, T first, Func<T, T, T> divide) : base(linear.Count)
        {
            _linear = linear;
            _first = first;
            _divide = divide;
        }

        protected override IEnumerable<T> Generate()
        {
            return _linear.Select(divisor => _divide(_first, divisor));
        }
    }
}
