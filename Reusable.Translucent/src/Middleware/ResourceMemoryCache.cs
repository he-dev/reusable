using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Reusable.Translucent.Data;

namespace Reusable.Translucent.Middleware
{
    /// <summary>
    /// Allows to cache resources for GET requests by specifying the MaxAge in the request.
    /// </summary>
    [UsedImplicitly]
    public class ResourceMemoryCache : MiddlewareBase
    {
        private readonly IMemoryCache _memoryCache;

        public ResourceMemoryCache(RequestDelegate<ResourceContext> next, IMemoryCache memoryCache) : base(next)
        {
            _memoryCache = memoryCache;
        }

        public override async Task InvokeAsync(ResourceContext context)
        {
            // Only GET requests are cacheable.
            if (context.Request.Method.Equals(ResourceMethod.Get))
            {
                // Only requests with non-zero MaxAge are cacheable.
                if (context.Request.MaxAge() is {} maxAge && maxAge > TimeSpan.Zero)
                {
                    context.Response = await _memoryCache.GetOrCreateAsync(context.Request.ResourceName, async entry =>
                    {
                        await InvokeNext(context);
                        // todo - copy stream to memory-stream
                        entry.Value = context.Response;
                        entry.AbsoluteExpirationRelativeToNow = maxAge;

                        return context.Response;
                    });
                }
                else
                {
                    await InvokeNext(context);
                }
            }
            else
            {
                await InvokeNext(context);
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
            return request.Items.TryGetValue("MaxAge", out var value) && value is TimeSpan maxAge ? maxAge : default(TimeSpan?);
        }
    }
}