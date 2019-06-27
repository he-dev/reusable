using System.Collections.Generic;

namespace Reusable.Collections.Generators
{
    public class MonotonicSequence<T> : Sequence<T>
    {
        public MonotonicSequence(T start, T step) : base(Create(start, step)) { }

        private static IEnumerable<T> Create(T start, T step)
        {
            foreach (var _ in Sequence.Infinite<T>())
            {
                yield return start;
                start = BinaryOperation<T>.Add(start, step);
            }
        }
    }
}