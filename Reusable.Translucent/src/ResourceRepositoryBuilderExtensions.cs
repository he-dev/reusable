using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Middleware;

namespace Reusable.Translucent
{
    public static class ResourceRepositoryBuilderExtensions
    {
        public static IPipelineBuilder<ResourceContext> UseResources(this IPipelineBuilder<ResourceContext> builder, Action<IResourceCollection> configure)
        {
            var resourceControllers = new ResourceCollection();
            configure(resourceControllers);

            return builder.UseMiddleware<ControllerMiddleware>(new object[] { resourceControllers });
        }
    }

    public interface IResourceCollection : IList<IResourceController>
    {
        IResourceCollection Add<T>(T controller) where T : IResourceController;
    }

    public class ResourceCollection : List<IResourceController>, IResourceCollection
    {
        public IResourceCollection Add<T>(T controller) where T : IResourceController
        {
            base.Add(controller);
            return this;
        }
    }


    public static class ResourceBuilderExtensions
    {
//        public static IResourceControllerCollection AddController(this IResourceControllerCollection builder, IResourceController controller)
//        {
//            builder.Add(controller);
//            return builder;
//        }
    }
}