using System;
using System.Diagnostics;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Modules;

public class SetElapsed : IModule
{
    public Func<TimeSpan, double> ToDouble { get; set; } = timeSpan => Math.Round(timeSpan.TotalSeconds, 3);

    public void Invoke(TraceContext context, LogFunc next)
    {
        var elapsed = context.Activity.Items.Stopwatch(Stopwatch.StartNew).Elapsed;
        context.Entry.Elapsed(ToDouble(elapsed));
        next(context);
    }
}