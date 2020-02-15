using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;
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

        public IResource Build(IServiceProvider? services = default)
        {
            return new Resource(services ?? ImmutableServiceProvider.Empty, s => _controller.Select(f => f(s)), s => _middleware.Select(f => f(s)));
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