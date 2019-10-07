using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Data;

namespace Reusable.Translucent.Middleware
{
    [UsedImplicitly]
    public class CacheMiddleware
    {
        private readonly RequestDelegate<ResourceContext> _next;
        private readonly IMemoryCache _memoryCache;

        public CacheMiddleware(RequestDelegate<ResourceContext> next, IMemoryCache memoryCache)
        {
            _next = next;
            _memoryCache = memoryCache;
        }

        public async Task InvokeAsync(ResourceContext context)
        {
            if (!context.Request.Method.Equals(RequestMethod.Get))
            {
                await _next(context);
                return;
            }

            // Only requests with non-zero MaxAge are cacheable.
            if (context.Request.Metadata.GetItemOrDefault(Resource.MaxAge, TimeSpan.Zero) is var maxAge && maxAge == TimeSpan.Zero)
            {
                await _next(context);
                return;
            }

            var key = context.Request.Uri.ToString();
            if (_memoryCache.TryGetValue<Response>(key, out var cached))
            {
                context.Response = cached;
            }
            else
            {
                await _next(context);
                _memoryCache.Set(key, context.Response, maxAge);
            }
        }
    }
}