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
    public class MemoryCache : MiddlewareBase
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCache(RequestDelegate<ResourceContext> next, IServiceProvider services) : base(next, services)
        {
            _memoryCache = Services.GetService<IMemoryCache>();
        }

        public override async Task InvokeAsync(ResourceContext context)
        {
            // Only GET requests are cacheable.
            if (!context.Request.Method.Equals(RequestMethod.Get))
            {
                await Next(context);
                return;
            }

            // Only requests with non-zero MaxAge are cacheable.
            if (context.Request.MaxAge == TimeSpan.Zero)
            {
                await Next(context);
                return;
            }

            var key = context.Request.Uri.ToString();
            if (_memoryCache.TryGetValue<Response>(key, out var cached))
            {
                context.Response = cached;
            }
            else
            {
                await Next(context);
                _memoryCache.Set(key, context.Response, context.Request.MaxAge);
            }
        }
    }
}