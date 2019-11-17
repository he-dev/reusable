using System;
using System.Threading.Tasks;
using Reusable.Middleware;

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
    }
}