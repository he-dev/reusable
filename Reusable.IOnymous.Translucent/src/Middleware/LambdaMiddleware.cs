using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous.Middleware
{
    [UsedImplicitly]
    public class LambdaMiddleware
    {
        private readonly RequestCallback<ResourceContext> _next;
        private readonly Func<ResourceContext, RequestCallback<ResourceContext>, Task> _lambda;

        public LambdaMiddleware(RequestCallback<ResourceContext> next, Func<ResourceContext, RequestCallback<ResourceContext>, Task> lambda)
        {
            _next = next;
            _lambda = lambda;
        }

        public Task InvokeAsync(ResourceContext context)
        {
            return _lambda(context, _next);
        }
    }
}