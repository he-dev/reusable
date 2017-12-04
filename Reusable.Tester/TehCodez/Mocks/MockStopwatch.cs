using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Tester.Mocks
{
    [PublicAPI]
    public class MockStopwatch : IStopwatch, IDisposable
    {
        public MockStopwatch(params TimeSpan[] elapsedTimes)
            : this(elapsedTimes.AsEnumerable())
        {
        }

        public MockStopwatch([NotNull] IEnumerable<TimeSpan> elapsedTimes)
        {
            if (elapsedTimes == null) throw new ArgumentNullException(nameof(elapsedTimes));
            ElapsedTimes = elapsedTimes.Prepend(TimeSpan.Zero).ToList().GetEnumerator();
        }

        private IEnumerator<TimeSpan> ElapsedTimes { get; }

        public bool IsRunning { get; private set; }

        public TimeSpan Elapsed => IsRunning ? NextElapsed() : ElapsedTimes.Current;

        [NotNull]
        public static MockStopwatch StartNew(IEnumerable<TimeSpan> elapses)
        {
            var stopwatch = new MockStopwatch(elapses);
            stopwatch.Start();
            return stopwatch;
        }

        public void Start()
        {
            IsRunning = true;
        }

        public void Stop()
        {
            IsRunning = false;
        }

        public void Restart()
        {
            Stop();
            Reset();
            Start();
        }

        public void Reset()
        {
            ElapsedTimes.Reset();
            ElapsedTimes.MoveNext();
        }

        private TimeSpan NextElapsed()
        {
            return
                ElapsedTimes.MoveNext()
                ? ElapsedTimes.Current
                : throw new InvalidOperationException("You did not define enough timestamps.");
        }

        public override string ToString()
        {
            return ElapsedTimes.Current.ToString();
        }

        public void Dispose()
        {
            ElapsedTimes.Dispose();
        }
    }
}
