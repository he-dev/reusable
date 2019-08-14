using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Middleware
{
    [UsedImplicitly]
    public class LambdaMiddleware<TContext>
    {
        private readonly RequestCallback<TContext> _next;
        private readonly Func<TContext, RequestCallback<TContext>, Task> _lambda;

        public LambdaMiddleware(RequestCallback<TContext> next, Func<TContext, RequestCallback<TContext>, Task> lambda)
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