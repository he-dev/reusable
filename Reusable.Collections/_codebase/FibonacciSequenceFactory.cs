using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Collections
{
    public class FibonacciSequenceFactory
    {
        public static FibonacciSequence<TimeSpan> Create(TimeSpan firstTwo, TimeSpan firstStep, int count)
        {
            return new FibonacciSequence<TimeSpan>(firstTwo, firstStep, count, (x, y) => x + y);
        }

        public static FibonacciSequence<int> Create(int firstTwo, int firstStep, int count)
        {
            return new FibonacciSequence<int>(firstTwo, firstStep, count, (x, y) => x + y);
        }
    }
}
