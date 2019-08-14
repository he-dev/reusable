using System;
using System.Threading.Tasks;
using Reusable.Middleware;

namespace Reusable
{
    public static class MiddlewareBuilderExtensions
    {
        public static MiddlewareBuilder Use<T>(this MiddlewareBuilder builder, params object[] parameters)
        {
            return builder.Add<T>(parameters);
        }

        public static MiddlewareBuilder Use<TContext>(this MiddlewareBuilder builder, Func<TContext, RequestCallback<TContext>, Task> lambda)
        {
            return builder.Add<LambdaMiddleware<TContext>>(lambda);
        }
    }
}