using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Reusable.Middleware;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Middleware;

namespace Reusable.Translucent
{
    public static class PipelineBuilderExtensions
    {
//        public static RequestDelegateBuilder UseMiddleware<T>(this RequestDelegateBuilder builder, params object[] parameters)
//        {
//            return builder.UseMiddleware<T>(parameters);
//        }

        public static IPipelineBuilder<TContext> UseMiddleware<TContext>(this IPipelineBuilder<TContext> builder, Func<TContext, RequestDelegate<TContext>, Task> lambda)
        {
            return builder.UseMiddleware<LambdaMiddleware<TContext>>(lambda);
        }

        public static IPipelineBuilder<TContext> UseResources<TContext>(this IPipelineBuilder<TContext> builder, IEnumerable<IResourceController> controllers)
        {
            return builder.UseMiddleware<ResourceMiddleware>(controllers);
        }

        public static IPipelineBuilder<TContext> UseResources<TContext>(this IPipelineBuilder<TContext> builder, params IResourceController[] controllers)
        {
            return builder.UseResources(controllers.AsEnumerable());
        }
    }
}