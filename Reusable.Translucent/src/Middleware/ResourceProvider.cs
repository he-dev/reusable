using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Extensions;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.SemanticExtensions;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;
using Reusable.Translucent.Extensions;

namespace Reusable.Translucent.Middleware
{
    /// <summary>
    /// Handles requests and determines which resource-controller to use to get a resources. Controllers determined with a GET requests are cached.
    /// </summary>
    [UsedImplicitly]
    public class ResourceProvider : ResourceMiddleware
    {
        private static readonly IEnumerable<ResourceControllerFilterDelegate> Filters = new ResourceControllerFilterDelegate[]
        {
            ResourceControllerFilters.FilterByControllerName,
            ResourceControllerFilters.FilterByRequest,
        };

        private readonly IImmutableList<IResourceController> controllers;
        private readonly ILogger? logger;
        private readonly IMemoryCache cache;

        public ResourceProvider
        (
            RequestDelegate next,
            ILogger<ResourceProvider> logger,
            IMemoryCache cache,
            IEnumerable<IResourceController> controllers
        ) : base(next)
        {
            this.logger = logger;
            this.controllers = controllers.ToImmutableList();
            this.cache = cache;
        }

        public override async Task InvokeAsync(ResourceContext context)
        {
            var providerKey = context.Request.ResourceName;

            // Used cached provider if already resolved.
            if (cache.TryGetValue<IResourceController>(providerKey, out var controllerFromCache))
            {
                //context.Response = await InvokeMethodAsync(entry, context.Request);
                context.Response = await controllerFromCache.InvokeAsync(context.Request);
            }
            else
            {
                var candidates = Filters.Aggregate(controllers.AsEnumerable(), (providers, filter) => filter(providers, context.Request));

                // READ can search multiple providers.
                if (context.Request.Method == ResourceMethod.Read)
                {
                    context.Response = Response.NotFound();
                    
                    foreach (var controller in candidates)
                    {
                        if (await controller.InvokeAsync(context.Request) is var response && response.Exists())
                        {
                            context.Response = response;
                            cache.Set(providerKey, controller);
                            logger?.Log(Abstraction.Layer.IO().Meta(new
                            {
                                resourceOk = new
                                {
                                    controller = controller.GetType().ToPrettyString(),
                                    name = controller.Name,
                                }
                            }));
                            break;
                        }
                        else
                        {
                            logger?.Log(Abstraction.Layer.IO().Meta(new
                            {
                                resourceNotFound = new
                                {
                                    controller = controller.GetType().ToPrettyString(),
                                    name = controller.Name,
                                }
                            }));
                        }
                    }
                }
                // Other methods are allowed to use only a single controller.
                else
                {
                    var controller = candidates.SingleOrThrow
                    (
                        onEmpty: ($"{nameof(ResourceController<Request>)}NotFound", $"Could not find controller for resource '{context.Request.ResourceName}'.")
                    );
                    
                    context.Response = await cache.Set(providerKey, controller).InvokeAsync(context.Request);
                }
            }
        }
    }
}