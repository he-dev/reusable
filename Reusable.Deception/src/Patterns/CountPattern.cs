using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.Deception.Patterns
{
    [UsedImplicitly]
    public class CountPattern : IterativePattern<int>
    {
        private int _counter;

        public CountPattern(IEnumerable<int> values) : base(values) { }

        protected override bool Matches() => ++_counter == Current;

        protected override void Reset() => _counter = 0;

        public override string ToString() => $"{nameof(CountPattern)}: '{_counter}'.";
    }
}