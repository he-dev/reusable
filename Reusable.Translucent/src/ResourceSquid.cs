using System;
using System.Threading.Tasks;

namespace Reusable.Translucent
{
    public interface IResourceSquid : IDisposable
    {
        Task<Response> InvokeAsync(Request request);
    }

    public class ResourceSquid : IResourceSquid
    {
        private readonly RequestCallback<ResourceContext> _requestCallback;

        public ResourceSquid(RequestCallback<ResourceContext> requestCallback)
        {
            _requestCallback = requestCallback;
        }

        public static ResourceSquidBuilder Builder => new ResourceSquidBuilder();

        public async Task<Response> InvokeAsync(Request request)
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
}