using System;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Marbles;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class AttachElapsed : IMiddleware
{
    public const string Key = "elapsed";
    
    public Func<IStopwatch> CreateStopwatch { get; set; } = () => new SystemStopwatch();
    
    public Func<TimeSpan, double> Format { get; set; } = timeSpan => Math.Round(timeSpan.TotalSeconds, 3);
    
    public void Invoke(LogEntry entry, Action<LogEntry> next)
    {
        var elapsed = entry.Contexts.First().GetOrCreate(Key, _ => CreateStopwatch()).Elapsed;
        next(entry.SetItem(Key, Format(elapsed)));
    }
}