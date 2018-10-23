using System;
using System.Collections.Generic;

namespace Reusable.Collections.Generators
{
    public class RandomSequence : Sequence<int>
    {
        private readonly Func<int> _next;

        public RandomSequence(int min, int max)
        {
            var random = new Random((int)DateTime.UtcNow.Ticks);
            _next = () => random.Next(min, max);
        }

        public override IEnumerator<int> GetEnumerator()
        {
            while (true) yield return _next();
            // ReSharper disable once IteratorNeverReturns - this is by design
        }
    }
}