using System.Collections.Generic;
using System.Linq;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Middleware;

namespace Reusable.Translucent
{
    public static class ResourceControllerExtensions
    {
        public static RequestDelegateBuilder<TContext> UseControllers<TContext>(this RequestDelegateBuilder<TContext> builder, IEnumerable<IResourceController> controllers)
        {
            return builder.UseMiddleware<ControllerMiddleware>(controllers);
        }

        public static RequestDelegateBuilder<TContext> UseControllers<TContext>(this RequestDelegateBuilder<TContext> builder, params IResourceController[] controllers)
        {
            return builder.UseControllers(controllers.AsEnumerable());
        }
    }
}