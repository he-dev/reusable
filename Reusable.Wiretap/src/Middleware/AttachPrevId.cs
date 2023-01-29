using System;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class AttachPrevId : IMiddleware
{
    public const string Key = "prevId";

    public void Invoke(LogEntry entry, Action<LogEntry> next)
    {
        if (entry.Contexts.Skip(1).FirstOrDefault() is { } cache)
        {
            entry = entry.SetItem(Key, cache.Get(AttachNodeId.Key));
        }

        next(entry);
    }
}