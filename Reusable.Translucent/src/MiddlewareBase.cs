using System;
using System.Threading.Tasks;

namespace Reusable.Translucent
{
    public interface IMiddleware
    {
        Task InvokeAsync(ResourceContext context);
    }
    
    public abstract class MiddlewareBase : IMiddleware
    {
        protected MiddlewareBase(RequestDelegate<ResourceContext> next, IServiceProvider services)
        {
            Next = next;
            Services = services;
        }

        private RequestDelegate<ResourceContext> Next { get; }

        protected IServiceProvider Services { get; }

        public abstract Task InvokeAsync(ResourceContext context);

        protected Task InvokeNext(ResourceContext context) => Next?.Invoke(context);
    }
}