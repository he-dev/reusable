using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Reusable.Synergy.Middleware;

public class CacheLifetimeService<T> : Service<T>
{
// Allows to associate cache-lifetime to with a service.
    public CacheLifetimeService(TimeSpan fallback)
    {
        if (fallback == TimeSpan.Zero) throw new ArgumentException("Fallback value needs to be greater than zero.");
        Fallback = fallback;
    }

    private TimeSpan Fallback { get; }

    public List<ConditionBag<TimeSpan>> Rules { get; } = new();

    public override async Task<T> InvokeAsync()
    {
        var lifetime = Rules.Where(c => c.Evaluate(Last)).Select(c => c.GetValue()).FirstOrDefault();

        Last.CacheLifetime(lifetime > TimeSpan.Zero ? lifetime : Fallback);

        return await InvokeNext();
    }
}