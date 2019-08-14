using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.IOnymous.Middleware;
using Reusable.Middleware;

namespace Reusable.IOnymous
{
    public static class ResourceControllerExtensions
    {
        public static MiddlewareBuilder UseControllers(this MiddlewareBuilder builder, IEnumerable<IResourceController> providers)
        {
            return builder.Add<ControllerMiddleware>(providers);
        }

        public static MiddlewareBuilder UseControllers(this MiddlewareBuilder builder, params IResourceController[] providers)
        {
            return builder.UseControllers(providers.AsEnumerable());
        }
        
        public static MiddlewareBuilder UseEnvironmentVariable(this MiddlewareBuilder builder)
        {
            return builder.Use<EnvironmentVariableMiddleware>();
        }
    }
}