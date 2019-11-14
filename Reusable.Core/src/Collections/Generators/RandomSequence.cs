using System;
using System.Linq;

namespace Reusable.Collections.Generators
{
    public class RandomSequence : Sequence<int>
    {
        public RandomSequence(int min, int max, Func<int, int, int> next)
            : base(Sequence.InfiniteDefault<int>().Select(_ => next(min, max))) { }

        public RandomSequence(int min, int max)
            : this(min, max, CreateNextFunc((int)DateTime.UtcNow.Ticks)) { }

        private static Func<int, int, int> CreateNextFunc(int seed)
        {
            var random = new Random(seed);
            return (min, max) => random.Next(min, max);
        }
    }
}