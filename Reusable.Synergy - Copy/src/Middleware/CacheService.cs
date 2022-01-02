using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;

namespace Reusable.Synergy.Middleware;

public class CacheService<T> : Service<T>
{
// Provides caching for service results.
    public CacheService(IMemoryCache cache, IPropertyService<string> key) => (Cache, Key) = (cache, key);

    private IMemoryCache Cache { get; }

    private IPropertyService<string> Key { get; }

    public override async Task<T> InvokeAsync()
    {
        if (Last.CacheLifetime() is var cacheLifetime && cacheLifetime > TimeSpan.Zero)
        {
            Console.WriteLine($"Cache-lifetime: {cacheLifetime}");
        }

        return await InvokeNext();
    }
}