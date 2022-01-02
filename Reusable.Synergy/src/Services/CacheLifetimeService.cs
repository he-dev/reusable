using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Reusable.Synergy.Services;

public class CacheLifetimeService : Service
{
// Allows to associate cache-lifetime to with a service.
    public CacheLifetimeService(TimeSpan fallback)
    {
        if (fallback == TimeSpan.Zero) throw new ArgumentException("Fallback value needs to be greater than zero.");
        Fallback = fallback;
    }

    private TimeSpan Fallback { get; }

    public List<ConditionBag<TimeSpan>> Rules { get; } = new();

    public override async Task<object> InvokeAsync(IRequest request)
    {
        var lifetime = Rules.Where(c => c.Evaluate(request)).Select(c => c.GetValue()).FirstOrDefault();

        request.CacheLifetime(lifetime > TimeSpan.Zero ? lifetime : Fallback);

        return await InvokeNext(request);
    }
}