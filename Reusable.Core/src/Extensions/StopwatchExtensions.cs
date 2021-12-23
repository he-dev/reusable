using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Reusable.Diagnostics
{
    public static class StopwatchExtensions
    {
        [CanBeNull]
        public static T Measure<T>([NotNull] this Stopwatch stopwatch, [NotNull] Func<T> action, [NotNull] Action<TimeSpan> elapsed)
        {
            if (stopwatch == null) throw new ArgumentNullException(nameof(stopwatch));
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (elapsed == null) throw new ArgumentNullException(nameof(elapsed));

            try
            {
                return action();
            }
            finally
            {
                elapsed(stopwatch.Elapsed);
            }
        }

        public static void Measure([NotNull] this Stopwatch stopwatch, [NotNull] Action action, [NotNull] Action<TimeSpan> elapsed)
        {
            if (stopwatch == null) throw new ArgumentNullException(nameof(stopwatch));
            if (action == null) throw new ArgumentNullException(nameof(action));
            if (elapsed == null) throw new ArgumentNullException(nameof(elapsed));

            stopwatch.Measure<object>(() => { action(); return default; }, elapsed);
        }
    }
}
