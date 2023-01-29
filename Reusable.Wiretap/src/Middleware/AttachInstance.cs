using System;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class AttachInstance : IMiddleware
{
    public const string Key = "instance";

    public string Name { get; set; } = "default"; 

    public void Invoke(LogEntry entry, Action<LogEntry> next)
    {
        next(entry.SetItem(Key, Name));
    }
}