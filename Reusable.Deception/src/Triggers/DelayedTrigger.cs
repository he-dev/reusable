using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using Reusable.Diagnostics.Abstractions;

namespace Reusable.Diagnostics.Triggers
{
    [UsedImplicitly]
    public class DelayedTrigger : PhantomExceptionTrigger
    {
        private Stopwatch _stopwatch;

        public DelayedTrigger(IEnumerable<int> sequence, int count = default) : base(sequence, count)
        {
        }

        private TimeSpan Delay => TimeSpan.FromSeconds(Current);

        protected override bool CanThrow()
        {
            _stopwatch = _stopwatch ?? Stopwatch.StartNew();
            if (_stopwatch.Elapsed >= Delay)
            {
                _stopwatch.Restart();
                return true;
            }

            return false;
        }

        public override string ToString() => $"{nameof(DelayedTrigger)}: {Delay} ({_stopwatch.Elapsed})";
    }
}