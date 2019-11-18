using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent.Middleware
{
    [UsedImplicitly]
    public class ControllerMiddleware
    {
        private static readonly IEnumerable<ResourceControllerFilterCallback> Filters = new ResourceControllerFilterCallback[]
        {
            ResourceControllerFilters.FilterByControllerId,
            ResourceControllerFilters.FilterByUriScheme,
            ResourceControllerFilters.FilterByControllerTags,
            ResourceControllerFilters.FilterByUriPath,
        };

        private readonly IImmutableList<IResourceController> _controllers;
        private readonly RequestDelegate<ResourceContext> _next;
        private readonly IMemoryCache _cache;

        public ControllerMiddleware(RequestDelegate<ResourceContext> next, IResourceCollection controllers)
        {
            _next = next;
            _controllers = controllers.ToImmutableList();
            _cache = new MemoryCache(new MemoryCacheOptions { });
        }

        public async Task InvokeAsync(ResourceContext context)
        {
            await _next(context);

            var providerKey = context.Request.Uri.ToString();

            // Used cached provider if already resolved.
            if (_cache.TryGetValue<IResourceController>(providerKey, out var entry))
            {
                context.Response = await InvokeMethodAsync(entry, context.Request);
            }
            else
            {
                var controllers = Filters.Aggregate(_controllers.AsEnumerable(), (providers, filter) => filter(providers, context.Request));

                // GET can search multiple providers.
                if (context.Request.Method == RequestMethod.Get)
                {
                    context.Response = Response.NotFound();
                    foreach (var controller in controllers)
                    {
                        if (await InvokeMethodAsync(controller, context.Request) is var response && response.Exists())
                        {
                            _cache.Set(providerKey, controller);
                            context.Response = response;
                            response.Metadata = response.Metadata.SetItem(Request.HandledBy, controller);
                            break;
                        }
                    }
                }
                // Other methods are allowed to use only a single controller.
                else
                {
                    var controller = _cache.Set(providerKey, controllers.SingleOrThrow(onEmpty: ($"{nameof(ResourceController)}NotFound", $"Could not find controller for resource '{context.Request.Uri}'.")));
                    context.Response = await InvokeMethodAsync(controller, context.Request);
                }
            }
        }

        private static Task<Response> InvokeMethodAsync(IResourceController controller, Request request)
        {
            var methods =
                controller
                    .GetType()
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(m => m.GetCustomAttribute<ResourceActionAttribute>()?.Method == request.Method);

            var method = methods.SingleOrThrow
            (
                onEmpty: ("MethodNotFound", $"Could not find method '{request.Method}' on controller '{controller.GetType().ToPrettyString()}'"),
                onMany: ("AmbiguousMethod", $"There is more than one method '{request.Method}' on controller '{controller.GetType().ToPrettyString()}'")
            );

            return (Task<Response>)method.Invoke(controller, new object[] { request });
        }
    }
}