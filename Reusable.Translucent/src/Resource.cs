using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;

namespace Reusable.Translucent
{
    [PublicAPI]
    public interface IResource
    {
        Task<Response> InvokeAsync(Request request);
    }

    [PublicAPI]
    public class Resource : IResource
    {
        private readonly IResourceMiddleware _resourceMiddleware;

        public Resource(IResourceMiddleware resourceMiddleware) => _resourceMiddleware = resourceMiddleware;

        public static ResourceBuilder Builder() => new ResourceBuilder();

        public async Task<Response> InvokeAsync(Request request)
        {
            var context = new ResourceContext {Request = request};
            await _resourceMiddleware.InvokeAsync(context);
            return context.Response;
        }
    }
    
    public delegate Task RequestDelegate(ResourceContext context);
    
    public delegate IResourceController CreateControllerDelegate();

    public delegate IResourceMiddleware CreateMiddlewareDelegate(RequestDelegate next);
}