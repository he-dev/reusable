using System;

namespace Reusable.Sequences
{
    public class FibonacciSequenceFactory
    {
        public static FibonacciSequence<TimeSpan> Create(int count, TimeSpan first)
        {
            return new FibonacciSequence<TimeSpan>(count, first, (x, y) => x + y);
        }

        public static FibonacciSequence<int> Create(int count, int first)
        {
            return new FibonacciSequence<int>(count, first, (x, y) => x + y);
        }

        public static FibonacciSequence<int> Create(int count)
        {
            return new FibonacciSequence<int>(count, 1, (x, y) => x + y);
        }
    }
}
