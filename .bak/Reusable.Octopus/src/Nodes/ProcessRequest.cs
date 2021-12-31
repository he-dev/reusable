using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Abstractions;
using Reusable.Octopus.Data;
using Reusable.Octopus.Extensions;

namespace Reusable.Octopus.Nodes;

/// <summary>
/// Handles requests and determines which resource-controller to use to get a resources. Controllers determined with a GET requests are cached.
/// </summary>
[UsedImplicitly]
public class ProcessRequest : ResourceNode
{
    public List<IResourceController> Controllers { get; set; } = new();

    private IMemoryCache Cache { get; set; } = new MemoryCache(new MemoryCacheOptions());

    public override async Task InvokeAsync(ResourceContext context)
    {
        var cacheKey = $"Resource['{context.Request.ResourceName}']";

        // Used cached controller if already resolved.
        if (context.Request.AllowControllerCaching && Cache.TryGetValue<IResourceController>(cacheKey, out var controllerFromCache))
        {
            context.Response = await controllerFromCache.InvokeAsync(context.Request);
        }
        else
        {
            var candidates = Controllers.Where(c => c.CanHandle(context.Request));

            // READ can search multiple controllers.
            if (context.Request.Method == RequestMethod.Read)
            {
                context.Request.Log($"Search '{context.Request.ResourceName}'...");
                context.Response = Response.NotFound(context.Request.ResourceName.Peek());

                foreach (var controller in candidates)
                {
                    context.Request.Log($"Invoke controller {controller.GetType().ToPrettyString()}...");
                    
                    if (await controller.InvokeAsync(context.Request) is { StatusCode: ResourceStatusCode.Success } response)
                    {
                        context.Response.Log($"...{ResourceStatusCode.Success}.");
                        context.Response = response;

                        if (context.Request.AllowControllerCaching)
                        {
                            Cache.Set(cacheKey, controller);
                        }

                        break;
                    }
                    else
                    {
                        context.Response.Log($"...{ResourceStatusCode.NotFound}.");
                    }
                }

                context.Response.Log("...done.");
            }
            // Other methods must match exactly one controller.
            else
            {
                var controller = candidates.SingleOrThrow($"Could not find unique controller for resource '{context.Request.ResourceName}'.");
                
                if (context.Request.AllowControllerCaching)
                {
                    Cache.Set(cacheKey, controller);
                }

                context.Response = await controller.InvokeAsync(context.Request);
            }
        }
    }
}

public static class ControllerExtensions
{
    public static bool CanHandle(this IResourceController controller, Request request)
    {
        return controller.Schema.Contains(request.Schema) && (request.ControllerFilter is null || request.ControllerFilter.Matches(controller));
    }
}