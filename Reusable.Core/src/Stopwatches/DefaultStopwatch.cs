using System;
using System.Diagnostics;
using Reusable.Extensions;

namespace Reusable
{
    public class DefaultStopwatch : IStopwatch
    {
        private readonly Stopwatch _stopwatch;

        public DefaultStopwatch() => _stopwatch = new Stopwatch();

        public bool IsRunning => _stopwatch.IsRunning;

        public TimeSpan Elapsed => _stopwatch.Elapsed;

        public static IStopwatch StartNew() => new DefaultStopwatch().Pipe(x => x.Start());

        public void Start() => _stopwatch.Start();

        public void Stop() => _stopwatch.Stop();

        public void Restart() => _stopwatch.Restart();

        public void Reset() => _stopwatch.Restart();

        public override string ToString() => _stopwatch.Elapsed.ToString();
    }
}