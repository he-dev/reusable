using System;
using System.Threading.Tasks;
using Reusable.Middleware;

namespace Reusable.Translucent
{
    public static class RequestDelegateBuilderExtensions
    {
        public static RequestDelegateBuilder UseMiddleware<T>(this RequestDelegateBuilder builder, params object[] parameters)
        {
            return builder.UseMiddleware<T>(parameters);
        }

        public static RequestDelegateBuilder UseMiddleware<TContext>(this RequestDelegateBuilder builder, Func<TContext, RequestDelegate<TContext>, Task> lambda)
        {
            return builder.UseMiddleware<LambdaMiddleware<TContext>>(lambda);
        }
    }
}