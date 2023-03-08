using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class AttachParent : IMiddleware
{
    public const string Key = "parent";

    public void Invoke(LogEntry entry, LogDelegate next)
    {
        if (entry.Context.Skip(1).FirstOrDefault() is { } context)
        {
            entry = entry.SetItem(Key, context.Properties.Scoped.Get<object>(AttachNode.Key));
        }

        next(entry);
    }
}