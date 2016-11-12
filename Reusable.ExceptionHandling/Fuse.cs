using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.ExceptionHandling
{
    public class Fuse
    {
        private IClock _clock = new SystemClock();

        public Fuse(Threshold threshold)
        {
            Threshold = threshold;
        }

        public IClock Clock
        {
            get { return _clock; }
            set
            {
                if (value == null) { throw new ArgumentNullException(nameof(Clock)); }
                _clock = value;
            }
        }

        public Threshold Threshold { get; }

        public bool AutoReset => Threshold.Timeout > TimeSpan.Zero;

        public int Count { get; private set; }

        public DateTime? StartedOn { get; private set; }

        public bool Blown => Clock.GetUtcNow() - StartedOn <= Threshold.Interval && Count >= Threshold.Count;

        public bool TimedOut => (Clock.GetUtcNow() - StartedOn) > Threshold.Timeout;

        public Fuse Pass(int value)
        {
            if (AutoReset && TimedOut) { Reset(); }
            Count += value;
            StartedOn = StartedOn ?? Clock.GetUtcNow();
            return this;
        }

        public Fuse PassOne()
        {
            Pass(1);
            return this;
        }

        public Fuse Reset()
        {
            Count = 0;
            StartedOn = null;
            return this;
        }

        public override string ToString()
        {
            return $"Count = {Count} Point = \"{StartedOn?.ToString(CultureInfo.InvariantCulture)}\" Blown = {Blown} TimedOut = {TimedOut}";
        }
    }
}
