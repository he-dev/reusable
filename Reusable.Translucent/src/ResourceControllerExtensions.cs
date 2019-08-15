using System.Collections.Generic;
using System.Linq;
using Reusable.Translucent.Middleware;

namespace Reusable.Translucent
{
    public static class ResourceControllerExtensions
    {
        public static RequestCallbackBuilder UseControllers(this RequestCallbackBuilder builder, IEnumerable<IResourceController> providers)
        {
            return builder.UseMiddleware<ControllerMiddleware>(providers);
        }

        public static RequestCallbackBuilder UseControllers(this RequestCallbackBuilder builder, params IResourceController[] providers)
        {
            return builder.UseControllers(providers.AsEnumerable());
        }
        
        // public static MiddlewareBuilder UseEnvironmentVariable(this MiddlewareBuilder builder)
        // {
        //     return builder.Use<EnvironmentVariableMiddleware>();
        // }
    }
}