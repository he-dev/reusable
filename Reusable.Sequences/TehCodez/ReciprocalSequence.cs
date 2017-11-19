using System.Collections.Generic;
using System.Linq;

namespace Reusable.Sequences
{
    public class ReciprocalSequence<T> : Sequence<T>
    {
        private readonly T _dividend;
        private readonly ISequence<T> _divisors;

        public ReciprocalSequence(T dividend, ISequence<T> divisors)
        {
            _dividend = dividend;
            _divisors = divisors;
        }

        public override IEnumerator<T> GetEnumerator()
        {
            return _divisors.Select(divisor => BinaryOperation<T>.Divide(_dividend, divisor)).GetEnumerator();
        }
    }
}
