using System;
using System.Diagnostics;
using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap.Modules;

public class SetElapsed : IModule
{
    public Func<TimeSpan, double> ToDouble { get; set; } = timeSpan => Math.Round(timeSpan.TotalSeconds, 3);

    public void Invoke(IActivity activity, LogEntry entry, LogFunc next)
    {
        var elapsed = activity.Items.Stopwatch(_ => Stopwatch.StartNew()).Elapsed;
        next(activity, entry.Elapsed(ToDouble(elapsed)));
    }
}