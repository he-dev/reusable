using System;
using System.Diagnostics;
using JetBrains.Annotations;

namespace Reusable.Marbles.Extensions;

[PublicAPI]
public static class StopwatchExtensions
{
    public static T? Measure<T>(this Stopwatch stopwatch, Func<T?> action, Action<TimeSpan> elapsed)
    {
        try
        {
            return action();
        }
        finally
        {
            elapsed(stopwatch.Elapsed);
        }
    }

    public static void Measure(this Stopwatch stopwatch, Action action, Action<TimeSpan> elapsed)
    {
        stopwatch.Measure<object>(() => { action(); return default; }, elapsed);
    }
}