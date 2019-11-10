using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Middleware;

namespace Reusable.Translucent
{
    public static class ResourceRepositoryBuilderExtensions
    {
        public static IResourceRepositoryBuilder<ResourceContext> UseResources(this IResourceRepositoryBuilder<ResourceContext> builder, Action<IResourceControllerBuilder> configure)
        {
            var resourceBuilder = new ResourceControllerBuilder(builder.ServiceProvider)
            {
                Controllers = new List<IResourceController>()
            };
            configure(resourceBuilder);

            return builder.UseMiddleware<ControllerMiddleware>(new object[] { resourceBuilder.Controllers.AsEnumerable() });
        }
    }

    internal class ResourceControllerBuilder : IResourceControllerBuilder
    {
        public ResourceControllerBuilder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }

        public IServiceProvider ServiceProvider { get; }

        public IList<IResourceController> Controllers { get; internal set; } = new List<IResourceController>();
    }

    public interface IResourceControllerBuilder
    {
        IServiceProvider ServiceProvider { get; }

        IList<IResourceController> Controllers { get; }
    }

    public static class ResourceBuilderExtensions
    {
        public static IResourceControllerBuilder AddController(this IResourceControllerBuilder builder, IResourceController controller)
        {
            builder.Controllers.Add(controller);
            return builder;
        }
    }
}