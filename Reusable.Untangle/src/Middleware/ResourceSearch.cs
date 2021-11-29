using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Extensions;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;
using Reusable.Translucent.Extensions;

namespace Reusable.Translucent.Middleware
{
    /// <summary>
    /// Handles requests and determines which resource-controller to use to get a resources. Controllers determined with a GET requests are cached.
    /// </summary>
    [UsedImplicitly]
    public class ResourceSearch : ResourceMiddleware
    {
        private static readonly IEnumerable<ResourceControllerFilterDelegate> Filters = new ResourceControllerFilterDelegate[]
        {
            ResourceControllerFilters.FilterByController,
            ResourceControllerFilters.FilterByRequest,
        };

        private readonly IEnumerable<IResourceController> _controllers;

        public ResourceSearch(IEnumerable<IResourceController> controllers) => _controllers = controllers;

        public IMemoryCache Cache { get; set; } = new MemoryCache(new MemoryCacheOptions());

        public override async Task InvokeAsync(ResourceContext context)
        {
            var cacheKey = $"Resource['{context.Request.ResourceName}']";

            // Used cached provider if already resolved.
            if (Cache.TryGetValue<IResourceController>(cacheKey, out var controllerFromCache))
            {
                //context.Response = await InvokeMethodAsync(entry, context.Request);
                context.Response = await controllerFromCache.InvokeAsync(context.Request);
            }
            else
            {
                var candidates = Filters.Aggregate(_controllers.AsEnumerable(), (providers, filter) => filter(providers, context.Request));

                // READ can search multiple providers.
                if (context.Request.Method == ResourceMethod.Read)
                {
                    context.Log.Add($"$Search '{context.Request.ResourceName}'...");
                    context.Response = Response.NotFound(context.Request.ResourceName);

                    foreach (var controller in candidates)
                    {
                        if (await controller.InvokeAsync(context.Request) is var response && response.Exists())
                        {
                            context.Log.Add($"{controller.GetType().ToPrettyString()}: {ResourceStatusCode.Success}.");
                            context.Response = response;
                            Cache.Set(cacheKey, controller);
                            break;
                        }
                        else
                        {
                            context.Log.Add($"{controller.GetType().ToPrettyString()}: {ResourceStatusCode.NotFound}.");
                        }
                    }
                    
                    context.Log.Add("...done.");
                }
                // Other methods are allowed to use only a single controller.
                else
                {
                    var controller = candidates.SingleOrThrow
                    (
                        onEmpty: ($"{nameof(ResourceController<Request>)}NotFound", $"Could not find controller for resource '{context.Request.ResourceName}'.")
                    );

                    context.Response = await Cache.Set(cacheKey, controller).InvokeAsync(context.Request);
                }
            }
        }
    }
}