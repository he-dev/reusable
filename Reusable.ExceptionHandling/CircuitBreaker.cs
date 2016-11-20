using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.ExceptionHandling
{
    public class CircuitBreaker
    {
        private IClock _clock = new SystemClock();

        public CircuitBreaker(Threshold threshold, TimeSpan timeout)
        {
            Threshold = threshold;
            Timeout = timeout;
        }

        public CircuitBreaker(Threshold threshold) : this(threshold, TimeSpan.Zero) { }

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

        public TimeSpan Timeout { get; set; }

        public bool AutoReset => Timeout > TimeSpan.Zero;

        public int Count { get; private set; }

        public DateTime? StartedOn { get; private set; }

        public CircutBreakerState State
        {
            get
            {
                var isOpen = Clock.GetUtcNow() - StartedOn <= Threshold.Interval && Count >= Threshold.Count;
                return isOpen ? CircutBreakerState.Open : CircutBreakerState.Closed;
            }
        }

        public bool TimedOut => (Clock.GetUtcNow() - StartedOn) > Timeout;

        public CircuitBreaker Pass(int value)
        {
            if (AutoReset && TimedOut) { Reset(); }
            Count += value;
            StartedOn = StartedOn ?? Clock.GetUtcNow();
            return this;
        }

        public CircuitBreaker PassOne()
        {
            Pass(1);
            return this;
        }

        public CircuitBreaker Reset()
        {
            if (!AutoReset) { throw new InvalidOperationException("Cannot reset. There is no timeout."); }
            Count = 0;
            StartedOn = null;
            return this;
        }

        public override string ToString()
        {
            return $"Count = {Count} Point = \"{StartedOn?.ToString(CultureInfo.InvariantCulture)}\" Blown = {State} TimedOut = {TimedOut}";
        }
    }
}
