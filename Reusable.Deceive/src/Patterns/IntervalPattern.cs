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

        protected override bool Matches()
        {
            if (_stopwatch.Elapsed >= Current)
            {
                _stopwatch.Restart();
                return true;
            }

            return false;
        }
    }
}