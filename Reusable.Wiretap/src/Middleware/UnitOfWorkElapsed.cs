using System;
using System.Diagnostics;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Middleware;

using static TimeSpanPrecision;

/// <summary>
/// This node adds 'Elapsed' time to the log.
/// </summary>
public class UnitOfWorkElapsed : LoggerMiddleware
{
    private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

    public UnitOfWorkElapsed()
    {
        Precision = Milliseconds;
        GetValue = ts => ts.ToDouble(Precision);
    }

    public TimeSpanPrecision Precision { get; set; }

    public Func<TimeSpan, double> GetValue { get; set; }

    public long Elapsed => (long)GetValue(_stopwatch.Elapsed);

    public override void Invoke(ILogEntry entry)
    {
        entry.Push(new LogProperty<IRegularProperty>(nameof(Elapsed), Elapsed));
        Next?.Invoke(entry);
    }

    public void Reset() => _stopwatch.Reset();
}