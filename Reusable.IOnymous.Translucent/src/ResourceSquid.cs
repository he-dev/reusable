using System;
using System.Collections;
using System.Threading.Tasks;

namespace Reusable.IOnymous
{
    public interface IResourceSquid : IDisposable
    {
        Task<IResource> InvokeAsync(Request request);
    }

    public class ResourceSquid : IResourceSquid
    {
        private readonly RequestCallback<ResourceContext> _requestCallback;

        public ResourceSquid(RequestCallback<ResourceContext> requestCallback)
        {
            _requestCallback = requestCallback;
        }

        public static ResourceSquidBuilder Builder => new ResourceSquidBuilder();

        public async Task<IResource> InvokeAsync(Request request)
        {
            var context = new ResourceContext
            {
                Request = request
            };

            await _requestCallback(context);

            return context.Response;
        }

        public void Dispose() { }
    }

    public class ResourceContext
    {
        public Request Request { get; set; }

        public IResource Response { get; set; }
    }
}