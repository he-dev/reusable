using System.Collections.Immutable;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class AttachCorrelationId : IMiddleware
{
    public const string Key = "correlationId";

    public void Invoke(LogEntry entry, LogDelegate next)
    {
        // Get the correlation-id from the nearest context.
        foreach (var context in entry.Context)
        {
            if (context.Properties.Scoped.TryGetItem<string>(Key, out var correlationId))
            {
                if (entry.TryGetValue("details", out var value) && value is IImmutableDictionary<string, object?> details)
                {
                    entry = entry.SetItem("details", details.SetItem("correlationId", correlationId));
                }

                break;
            }
        }

        next(entry);
    }
}