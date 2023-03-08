using System;
using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class AttachNode : IMiddleware
{
    public const string Key = "node";

    /// <summary>
    /// Gets or sets the factory for the correlation-id. Uses a continuous GUID by default.
    /// </summary>
    public Func<object> NewId { get; set; } = () => Guid.NewGuid();

    public void Invoke(LogEntry entry, LogDelegate next)
    {
        var id = entry.Context.First().Properties.Scoped.GetOrAdd(Key, () => NewId());
        next(entry.SetItem(Key, id));
    }
}