using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Reusable.Synergy.Services;

// Allows to associate cache-lifetime to with a service.
public class CacheLifetimeService : Service
{
    public CacheLifetimeService(TimeSpan value)
    {
        if (value == TimeSpan.Zero) throw new ArgumentException("Cache lifetime needs to be greater than zero.");
        Value = value;
    }

    private TimeSpan Value { get; }

    public override async Task<object> InvokeAsync(IRequest request)
    {
        request.CacheLifetime(Value);

        return await InvokeNext(request);
    }
}