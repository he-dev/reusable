using System.Collections.Generic;
using JetBrains.Annotations;
using Reusable.Diagnostics.Abstractions;

namespace Reusable.Diagnostics.Triggers
{
    [UsedImplicitly]
    public class CountedTrigger : PhantomExceptionTrigger
    {
        private int _counter;

        public CountedTrigger(IEnumerable<int> sequence, int count = default) : base(sequence, count)
        {
        }

        private string DebuggerDisplay => this.ToDebuggerDisplayString(b =>
        {
            b.DisplayMember(x => x._counter);
            b.DisplayMember(x => x.Current);
        });

        protected override bool CanThrow()
        {
            if (++_counter == Current)
            {
                _counter = 0;
                return true;
            }

            return false;
        }

        public override string ToString() => $"{nameof(CountedTrigger)}: {_counter}";
    }
}