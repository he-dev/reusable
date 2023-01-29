using System;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class AttachNodeId : IMiddleware
{
    public const string Key = "nodeId";

    /// <summary>
    /// Gets or sets the factory for the correlation-id. Uses a continuous GUID by default.
    /// </summary>
    public Func<object> NewId { get; set; } = () => Guid.NewGuid();

    public void Invoke(LogEntry entry, Action<LogEntry> next)
    {
        var id = entry.Contexts.First().GetOrCreate(Key, _ => NewId());
        next(entry.SetItem(Key, id));
    }
}