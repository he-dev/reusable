using System.Threading.Tasks;
using Reusable.Collections.Generic;

namespace Reusable.Translucent.Abstractions
{
    public interface IResourceMiddleware : INode<IResourceMiddleware>
    {
        Task InvokeAsync(ResourceContext context);
    }

    public abstract class ResourceMiddleware : IResourceMiddleware
    {
        public IResourceMiddleware? Prev { get; set; }

        public IResourceMiddleware? Next { get; set; }

        public abstract Task InvokeAsync(ResourceContext context);

        protected Task InvokeNext(ResourceContext context) => Next is {} node ? node.InvokeAsync(context) : Task.CompletedTask;
    }
}