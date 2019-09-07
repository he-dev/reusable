using System.Collections.Generic;
using System.Linq;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Middleware;

namespace Reusable.Translucent
{
    public static class ResourceControllerExtensions
    {
        public static RequestDelegateBuilder UseControllers(this RequestDelegateBuilder builder, IEnumerable<IResourceController> controllers)
        {
            return builder.UseMiddleware<ResourceMiddleware>(controllers);
        }

        public static RequestDelegateBuilder UseControllers(this RequestDelegateBuilder builder, params IResourceController[] controllers)
        {
            return builder.UseControllers(controllers.AsEnumerable());
        }
    }
}