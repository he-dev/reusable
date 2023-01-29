using System;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class AttachScope : IMiddleware
{
    public const string Key = "scope";

    public void Invoke(LogEntry entry, Action<LogEntry> next)
    {
        var name = entry.Contexts.First().Get(nameof(UnitOfWork));
        next(entry.SetItem(Key, name));
    }
}