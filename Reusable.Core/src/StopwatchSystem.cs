using System;
using System.Diagnostics;

namespace Reusable
{
    public class StopwatchDefault : IStopwatch
    {
        private readonly Stopwatch _stopwatch;

        public StopwatchDefault() => _stopwatch = new Stopwatch();

        public bool IsRunning => _stopwatch.IsRunning;

        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public static IStopwatch StartNew()
        {
            var stopwatch = new StopwatchDefault();
            stopwatch.Start();
            return stopwatch;
        }

        public void Start() => _stopwatch.Start();

        public void Stop() => _stopwatch.Stop();

        public void Restart() => _stopwatch.Restart();

        public void Reset() => _stopwatch.Restart();

        public override string ToString() => _stopwatch.Elapsed.ToString();
    }
}
