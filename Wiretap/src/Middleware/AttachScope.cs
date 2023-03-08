using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap.Middleware;

public class AttachScope : IMiddleware
{
    public const string Key = "scope";

    public void Invoke(LogEntry entry, LogDelegate next)
    {
        var name = entry.Context.First().Properties.Scoped.Get<string>(Key);
        next(entry.SetItem(Key, name));
    }
}