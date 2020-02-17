using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Collections.Generic;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;
using Reusable.Translucent.Middleware;

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

        public Resource(IEnumerable<IResourceMiddleware> middleware) => _resourceMiddleware = middleware.Chain().Head();

        public Resource(IEnumerable<IResourceController> controllers) : this(new IResourceMiddleware[] { new ResourceSearch(controllers) }) { }

        public async Task<Response> InvokeAsync(Request request)
        {
            var context = new ResourceContext { Request = request };
            await _resourceMiddleware.InvokeAsync(context);
            return context.Response;
        }
    }

    //public delegate Task RequestDelegate(ResourceContext context);

    //public delegate IResourceController CreateControllerDelegate();

    //public delegate IResourceMiddleware CreateMiddlewareDelegate();
}