using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Reusable.Exceptionize;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Middleware;

namespace Reusable.Translucent
{
    public class ResourceBuilder
    {
        private readonly List<Func<IServiceProvider, CreateControllerDelegate>> _controller = new List<Func<IServiceProvider, CreateControllerDelegate>>();
        private readonly List<Func<IServiceProvider, CreateMiddlewareDelegate>> _middleware = new List<Func<IServiceProvider, CreateMiddlewareDelegate>>();

        public ResourceBuilder UseController(Func<IServiceProvider, CreateControllerDelegate> factory)
        {
            _controller.Add(factory);
            return this;
        }

        public ResourceBuilder UseMiddleware(Func<IServiceProvider, CreateMiddlewareDelegate> factory)
        {
            _middleware.Add(factory);
            return this;
        }

        private static IEnumerable<CreateMiddlewareDelegate> CreateDefaultMiddleware(IServiceProvider serviceProvider, IEnumerable<IResourceController> controllers)
        {
            yield return next => new ResourceProvider
            (
                next,
                (serviceProvider.GetService<ILoggerFactory>() ?? LoggerFactory.Empty()).CreateLogger<ResourceProvider>(),
                (serviceProvider.GetService<IMemoryCache>() ?? new MemoryCache(new MemoryCacheOptions())),
                controllers
            );
        }

        public IResource Build(IServiceProvider? serviceProvider = default)
        {
            serviceProvider ??= ImmutableServiceProvider.Empty;

            var middlewareFactories = 
                _middleware
                    .Select(f => f(serviceProvider))
                    .Concat(CreateDefaultMiddleware(serviceProvider, _controller.Select(f => f(serviceProvider)())))
                    .ToStack();

            var resourceMiddleware = middlewareFactories.Aggregate(default(IResourceMiddleware?), (previous, factory) =>
            {
                try
                {
                    return factory(request => previous?.InvokeAsync(request) ?? Task.CompletedTask);
                }
                catch (Exception inner)
                {
                    throw DynamicException.Create("ResourceMiddlewareActivation", $"Could not activate middleware. See the inner exception for details", inner);
                }
            });

            return new Resource(resourceMiddleware);
        }
    }

    public static class RepositoryBuilderExtensions
    {
        public static ResourceBuilder UseController(this ResourceBuilder builder, IResourceController controller)
        {
            return builder.UseController(_ => () => controller);
        }

        public static ResourceBuilder UseMiddleware(this ResourceBuilder builder, Func<IServiceProvider, CreateMiddlewareDelegate> factory)
        {
            return builder.UseMiddleware(factory);
        }
    }
}