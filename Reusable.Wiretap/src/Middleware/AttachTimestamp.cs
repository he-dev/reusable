using System;
using Reusable.Marbles;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class AttachTimestamp : IMiddleware
{
    public const string Key = "timestamp";

    public Func<DateTime> GetDateTime { get; set; } = () => new DateTimeUtc().Now();

    public void Invoke(LogEntry entry, Action<LogEntry> next)
    {
        next(entry.SetItem(Key, GetDateTime()));
    }
}