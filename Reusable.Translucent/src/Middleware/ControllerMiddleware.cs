using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Exceptionize;

namespace Reusable.Translucent.Middleware
{
    [UsedImplicitly]
    public class ControllerMiddleware
    {
        private static readonly IEnumerable<ResourceControllerFilterCallback> Filters = new ResourceControllerFilterCallback[]
        {
            ResourceControllerFilters.FilterByUriScheme,
            ResourceControllerFilters.FilterByUriPath,
            ResourceControllerFilters.FilterByControllerTags,
        };

        private readonly IImmutableList<IResourceController> _controllers;
        private readonly RequestCallback<ResourceContext> _next;
        private readonly IMemoryCache _cache;

        public ControllerMiddleware(RequestCallback<ResourceContext> next, IEnumerable<IResourceController> controllers)
        {
            _next = next;
            _controllers = controllers.ToImmutableList();
            _cache = new MemoryCache(new MemoryCacheOptions { });
        }

        public async Task InvokeAsync(ResourceContext context)
        {
            var providerKey = context.Request.Uri.ToString();

            // Used cached provider if already resolved.
            if (_cache.TryGetValue<IResourceController>(providerKey, out var entry))
            {
                context.Response = await InvokeAsync(entry, context.Request);
            }
            else
            {
                var filtered = Filters.Aggregate(_controllers.AsEnumerable(), (providers, filter) => filter(providers, context.Request));

                // GET can search multiple providers.
                if (context.Request.Method == RequestMethod.Get)
                {
                    context.Response = new Response.NotFound();
                    foreach (var provider in filtered)
                    {
                        if (await InvokeAsync(provider, context.Request) is var response && response.StatusCode == ResourceStatusCode.OK)
                        {
                            _cache.Set(providerKey, provider);
                            context.Response = response;
                            break;
                        }
                    }
                }
                // Other methods are allowed to use only a single provider.
                else
                {
                    var resourceProvider =
                        _cache.Set(
                            providerKey,
                            filtered.SingleOrThrow(
                                onEmpty: () => DynamicException.Create("ResourceProviderNotFound", $"Could not get resource '{context.Request.Uri.ToString()}'.")));
                    context.Response = await InvokeAsync(resourceProvider, context.Request);
                }
            }

            await _next(context);
            //context.Response.Uri = 
        }

        private Task<Response> InvokeAsync(IResourceController controller, Request request)
        {
            var method =
                controller
                    .GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .SingleOrDefault(m => m.GetCustomAttribute<ResourceActionAttribute>()?.Method == request.Method);

            return (Task<Response>)method.Invoke(controller, new object[] { request });
        }
    }
}