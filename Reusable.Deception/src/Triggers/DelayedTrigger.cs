using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Reusable.Deception.Triggers
{
    [UsedImplicitly]
    public class DelayedTrigger : PhantomExceptionTrigger<TimeSpan>
    {
        private Stopwatch _stopwatch;

        public DelayedTrigger(IEnumerable<TimeSpan> values):base(values) { }

        protected override bool CanThrow() => _stopwatch.Elapsed >= Current;

        protected override void Reset() => _stopwatch = Stopwatch.StartNew();

        public override string ToString() => $"{nameof(DelayedTrigger)}: {Current} ({_stopwatch.Elapsed})";
    }
}