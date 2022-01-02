using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Reusable.Synergy.Services;

// Provides caching for service results.
public class CacheService : Service
{
    public CacheService(IMemoryCache cache, IPropertyAccessor<string> key) => (Cache, Key) = (cache, key);

    private IMemoryCache Cache { get; }

    private IPropertyAccessor<string> Key { get; }

    public override async Task<object> InvokeAsync(IRequest request)
    {
        if (request.CacheLifetime() is var cacheLifetime && cacheLifetime > TimeSpan.Zero)
        {
            Console.WriteLine($"Cache-lifetime: {cacheLifetime}");
        }

        return await InvokeNext(request);
    }
}