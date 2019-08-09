using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Reusable.Deception.Patterns
{
    [UsedImplicitly]
    public class IntervalPattern : IterativePattern<TimeSpan>
    {
        private readonly Stopwatch _stopwatch;

        public IntervalPattern(IEnumerable<TimeSpan> values) : base(values)
        {
            _stopwatch = Stopwatch.StartNew();
        }

        protected override bool Matches() => _stopwatch.Elapsed >= Current;

        protected override void Reset() => _stopwatch.Restart();

        public override string ToString() => $"{nameof(IntervalPattern)}: '{Current}'.";
    }
}