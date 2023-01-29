using System;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class AttachProperty : IMiddleware
{
    public AttachProperty(string key, object value) => (Key, Value) = (key, value);

    private string Key { get; }

    private object Value { get; }

    public void Invoke(LogEntry entry, Action<LogEntry> next)
    {
        next(entry.SetItem(Key, Value));
    }
}