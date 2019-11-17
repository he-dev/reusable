using System.Collections.Generic;
using System.Linq;
using Reusable.Data;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Middleware;

namespace Reusable.Translucent
{
    public static class ResourceControllerExtensions
    {
        public static IPipelineBuilder<TContext> UseControllers<TContext>(this IPipelineBuilder<TContext> builder, IEnumerable<IResourceController> controllers)
        {
            return builder.UseMiddleware<ControllerMiddleware>(controllers);
        }

        public static IPipelineBuilder<TContext> UseControllers<TContext>(this IPipelineBuilder<TContext> builder, params IResourceController[] controllers)
        {
            return builder.UseControllers(controllers.AsEnumerable());
        }
        
        public static bool SupportsRelativeUri(this IResourceController resourceController)
        {
            return resourceController.Properties.GetItemOrDefault(ResourceController.BaseUri) is {};
        }
    }
}