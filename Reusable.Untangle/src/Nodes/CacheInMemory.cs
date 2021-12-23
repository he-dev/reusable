using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Essentials.Extensions;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;
using Reusable.Translucent.Extensions;

namespace Reusable.Translucent.Nodes;

/// <summary>
/// Allows to cache resources for READ requests by specifying the cache-lifetime in the request.
/// </summary>
[UsedImplicitly]
public class CacheInMemory : ResourceNode
{
    public CacheInMemory(IMemoryCache memoryCache) => MemoryCache = memoryCache;

    private IMemoryCache MemoryCache { get; }

    public override async Task InvokeAsync(ResourceContext context)
    {
        // Only READ requests are cacheable.
        if (context.Request.Method == RequestMethod.Read)
        {
            // Only requests with non-zero CacheLifetime are cacheable.
            if (context.Request.CacheLifetime() is var lifetime && lifetime > TimeSpan.Zero)
            {
                var cacheKey = $"{context.Request.GetType().ToPrettyString()}/{context.Request.ResourceName}";

                // Use the cached value.
                if (MemoryCache.TryGetValue(cacheKey, out var cached))
                {
                    if (cached is Stream stream)
                    {
                        var copy = new MemoryStream();
                        await stream.CopyToAsync(copy);
                        context.Response.Body.Push(copy);
                    }

                    if (cached is string text)
                    {
                        context.Response.Body.Push(text);
                    }
                }
                // Proceed with the request. 
                else
                {
                    await InvokeNext(context);

                    if (context.Response is { } response && response.Body.Peek() is {} body)
                    {
                        var entry = MemoryCache.CreateEntry(cacheKey);
                        entry.AbsoluteExpirationRelativeToNow = lifetime;
                        
                        if (body is Stream stream)
                        {
                            var copy = new MemoryStream();
                            await stream.CopyToAsync(copy);
                            entry.Value = copy;
                        }

                        if (body is string text)
                        {
                            entry.Value = text;
                        }
                    }
                }
            }
            // Request is not cacheable.
            else
            {
                await InvokeNext(context);
            }
        }
        // Request is not a READ method.
        else
        {
            await InvokeNext(context);
        }
    }
}