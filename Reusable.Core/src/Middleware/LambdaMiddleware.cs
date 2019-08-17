using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Middleware
{
    [UsedImplicitly]
    public class LambdaMiddleware<TContext>
    {
        private readonly RequestDelegate<TContext> _next;
        private readonly Func<TContext, RequestDelegate<TContext>, Task> _lambda;

        public LambdaMiddleware(RequestDelegate<TContext> next, Func<TContext, RequestDelegate<TContext>, Task> lambda)
        {
            _next = next;
            _lambda = lambda;
        }

        public Task InvokeAsync(TContext context)
        {
            return _lambda(context, _next);
        }
    }
}