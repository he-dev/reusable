using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Middleware
{
    /// <summary>
    /// Allows to cache resources for GET requests by specifying the MaxAge in the request.
    /// </summary>
    [UsedImplicitly]
    public class ResourceMemoryCache : ResourceMiddleware
    {
        private readonly IMemoryCache _memoryCache;

        public ResourceMemoryCache(RequestDelegate<ResourceContext> next, IMemoryCache memoryCache) : base(next)
        {
            _memoryCache = memoryCache;
        }

        public override async Task InvokeAsync(ResourceContext context)
        {
            // Only GET requests are cacheable.
            if (context.Request.Method == ResourceMethod.Read)
            {
                // Only requests with non-zero MaxAge are cacheable.
                if (context.Request.MaxAge() is {} maxAge && maxAge > TimeSpan.Zero)
                {
                    context.Response = await _memoryCache.GetOrCreateAsync(context.Request.ResourceName, async entry =>
                    {
                        await Next(context);

                        if (context.Response.Body is Stream stream)
                        {
                            var copy = new MemoryStream();
                            using (stream)
                            {
                                await stream.CopyToAsync(copy);
                            }

                            context.Response.Body = copy;
                        }
                        
                        entry.Value = context.Response;
                        entry.AbsoluteExpirationRelativeToNow = maxAge;

                        return context.Response;
                    });
                }
                else
                {
                    await Next(context);
                }
            }
            else
            {
                await Next(context);
            }
        }
    }

    public static class ResourceMemoryCacheHelper
    {
        public static void MaxAge(this Request request, TimeSpan maxAge)
        {
            request.Items[nameof(MaxAge)] = maxAge;
        }

        public static TimeSpan? MaxAge(this Request request)
        {
            return request.Items.TryGetValue(nameof(MaxAge), out var value) && value is TimeSpan maxAge ? maxAge : default(TimeSpan?);
        }
    }
}