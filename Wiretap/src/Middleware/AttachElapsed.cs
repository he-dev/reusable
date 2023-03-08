using System;
using System.Diagnostics;
using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class AttachElapsed : IMiddleware
{
    public const string Key = "elapsed";
    
    public Func<Stopwatch> CreateStopwatch { get; set; } = Stopwatch.StartNew;
    
    public Func<TimeSpan, double> Format { get; set; } = timeSpan => Math.Round(timeSpan.TotalSeconds, 3);
    
    public void Invoke(LogEntry entry, LogDelegate next)
    {
        var elapsed = entry.Context.First().Properties.Scoped.GetOrAdd(Key, () => CreateStopwatch()).Elapsed;
        next(entry.SetItem(Key, Format(elapsed)));
    }
}