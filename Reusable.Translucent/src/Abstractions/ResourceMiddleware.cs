using System.Threading.Tasks;

namespace Reusable.Translucent.Abstractions
{
    public interface IResourceMiddleware
    {
        Task InvokeAsync(ResourceContext context);
    }

    public abstract class ResourceMiddleware : IResourceMiddleware
    {
        protected ResourceMiddleware(RequestDelegate<ResourceContext> next) => Next = next;

        protected RequestDelegate<ResourceContext> Next { get; }

        public abstract Task InvokeAsync(ResourceContext context);
    }
}