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
        private readonly IResourceMiddleware _middleware;

        public Resource(IEnumerable<IResourceMiddleware> middleware) => _middleware = middleware.Chain().Head();

        public Resource(IEnumerable<IResourceController> controllers) : this(new IResourceMiddleware[] { new ResourceSearch(controllers) }) { }

        public async Task<Response> InvokeAsync(Request request)
        {
            var context = new ResourceContext { Request = request };
            await _middleware.InvokeAsync(context);
            return context.Response!;
        }
    }
}