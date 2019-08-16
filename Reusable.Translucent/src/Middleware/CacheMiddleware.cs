using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Data;

namespace Reusable.Translucent.Middleware
{
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
            if (context.Request.Metadata.GetItemOrDefault(Request.IsCacheable))
            {
                var key = context.Request.Uri.ToString();
                if (_memoryCache.TryGetValue(key, out var cached))
                {
                    context.Response = (Response)cached;
                }
                else
                {
                    await _next(context);

                    _memoryCache.Set(key, context.Response, context.Request.Metadata.GetItemOrDefault(Request.CacheTimeout, TimeSpan.Zero));
                    context.Response.Metadata = context.Response.Metadata.SetItem(Request.IsExternallyOwned, true);
                }
            }
        }
    }
}