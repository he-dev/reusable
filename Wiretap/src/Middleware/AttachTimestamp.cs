using System;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class AttachTimestamp : IMiddleware
{
    public const string Key = "timestamp";

    public Func<DateTime> Now { get; set; } = () => DateTime.UtcNow;

    public void Invoke(LogEntry entry, LogDelegate next)
    {
        next(entry.SetItem(Key, Now()));
    }
}