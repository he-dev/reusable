using System;
using System.Threading.Tasks;
using Reusable.Middleware;

namespace Reusable.Translucent
{
    public static class RequestCallbackBuilderExtensions
    {
        public static RequestCallbackBuilder Use<T>(this RequestCallbackBuilder builder, params object[] parameters)
        {
            return builder.UseMiddleware<T>(parameters);
        }

        public static RequestCallbackBuilder Use<TContext>(this RequestCallbackBuilder builder, Func<TContext, RequestCallback<TContext>, Task> lambda)
        {
            return builder.UseMiddleware<LambdaMiddleware<TContext>>(lambda);
        }
    }
}