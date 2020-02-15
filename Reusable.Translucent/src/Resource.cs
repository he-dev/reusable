using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Reflection;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Reusable.Exceptionize;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.Translucent.Data;
using Reusable.Translucent.Middleware;

namespace Reusable.Translucent
{
    [PublicAPI]
    public interface IResource
    {
        Task<Response> InvokeAsync(Request request);
    }

    [PublicAPI]
    public class Resource : IResource
    {
        private readonly IMiddleware _middleware;

        public Resource(IServiceProvider serviceProvider, ControllerFactory controllerFactory, MiddlewareFactory? middlewareFactory = default)
        {
            middlewareFactory ??= _ => Enumerable.Empty<CreateMiddlewareDelegate>();

            var controllerFactories = controllerFactory(serviceProvider);
            var middlewareFactories =
                middlewareFactory(serviceProvider)
                    .Prepend(next => new ResourceControllerSwitch(
                        next,
                        (serviceProvider.GetService<ILoggerFactory>() ?? LoggerFactory.Empty()).CreateLogger<ResourceControllerSwitch>(),
                        (serviceProvider.GetService<IMemoryCache>() ?? new MemoryCache(new MemoryCacheOptions())),
                        controllerFactories.Select(f => f())));

            _middleware = middlewareFactories.Aggregate(default(IMiddleware?), (previous, factory) =>
            {
                try
                {
                    return factory(request => previous?.InvokeAsync(request) ?? Task.CompletedTask);
                }
                catch (Exception inner)
                {
                    throw DynamicException.Create("MiddlewareActivation", $"Could not activate middleware. See the inner exception for details", inner);
                }
            });
        }
        
        public static ResourceBuilder Builder() => new ResourceBuilder();

        public async Task<Response> InvokeAsync(Request request)
        {
            var context = new ResourceContext
            {
                Request = request
            };

            await _middleware.InvokeAsync(context);

            return context.Response;
        }
    }
}