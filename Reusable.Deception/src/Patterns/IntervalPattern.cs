using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Reusable.Deception.Patterns
{
    [UsedImplicitly]
    public class IntervalPattern : PhantomExceptionPattern<TimeSpan>
    {
        private Stopwatch _stopwatch;

        public IntervalPattern(IEnumerable<TimeSpan> values) : base(values) { }

        protected override bool Matches() => _stopwatch.Elapsed >= Current;

        protected override void Reset() => _stopwatch = Stopwatch.StartNew();

        public override string ToString() => $"{nameof(IntervalPattern)}: {Current} at ({_stopwatch.Elapsed})";
    }
}