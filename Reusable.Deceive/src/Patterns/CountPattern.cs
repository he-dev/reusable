using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Deception.Patterns
{
    [UsedImplicitly]
    public class CountPattern : IterativePattern<int>
    {
        private int _counter;

        public CountPattern(IEnumerable<int> values) : base(values) { }

        protected override bool Matches()
        {
            if (++_counter == Current)
            {
                _counter = 0;
                return true;
            }

            return false;
        }
    }
}