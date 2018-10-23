using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using JetBrains.Annotations;
using Reusable.Collections;

namespace Reusable
{
    public class AsyncDetector
    {
        private readonly ConcurrentBag<AsyncScope> _runtimes;
        private readonly IStopwatch _stopwatch;

        public AsyncDetector([NotNull] IStopwatch stopwatch)
        {
            _runtimes = new ConcurrentBag<AsyncScope>();
            _stopwatch = stopwatch ?? throw new ArgumentNullException(nameof(stopwatch));
            _stopwatch.Start();
        }

        public AsyncDetector() : this(new StopwatchDefault())
        {
        }

        public int MaxAsyncDegree
        {
            get
            {
                return
                    _runtimes
                        .GroupBy(t => t, AsyncScope.Comparer)
                        .Select(t => t.Count())
                        .Max();
            }
        }

        public IEnumerable<int> AllAsyncDegrees
        {
            get
            {
                return
                    _runtimes
                        .GroupBy(t => t, AsyncScope.Comparer)
                        .Select(t => t.Count());
            }
        }

        public int AsyncGroupCount
        {
            get
            {
                return
                    _runtimes
                        .GroupBy(t => t, AsyncScope.Comparer).Count();
            }
        }

        public IDisposable BeignScope()
        {
            return new AsyncScope(_stopwatch, scope => _runtimes.Add(scope));
        }

        // ReSharper disable once UnusedMember.Local - this is for LINQPad
        private object ToDump() => new { MaxAsyncDegree, AsyncGroupCount };

    }

    internal class AsyncScope : IDisposable
    {
        //static AsyncDetector()
        //{
        //    //var a = left.Min.Ticks;
        //    //var b = left.Max.Ticks;
        //    //var c = right.Min.Ticks;
        //    //var d = right.Max.Ticks;

        //    //return
        //    //    (a <= c && c <= b) ||
        //    //    (a <= d && d <= b);
        //    Comparer = AdHocEqualityComparer<Range<TimeSpan>>.CreateWithoutHashCode((left, right) => left.OverlapsInclusive(right));
        //}

        public static readonly IEqualityComparer<AsyncScope> Comparer = EqualityComparerFactory<AsyncScope>.Create((left, right) => left.Interval.OverlapsInclusive(right.Interval));

        private readonly IStopwatch _stopwatch;
        private readonly Action<AsyncScope> _dispose;

        public AsyncScope(IStopwatch stopwatch, Action<AsyncScope> dispose)
        {
            _stopwatch = stopwatch;
            _dispose = dispose;
            Interval = Range.Create(_stopwatch.Elapsed);
        }

        public Range<TimeSpan> Interval { get; private set; }

        public void Dispose()
        {
            Interval = Range.Create(Interval.Min, _stopwatch.Elapsed);
            _dispose(this);
        }
    }
}
