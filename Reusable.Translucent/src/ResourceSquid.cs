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
        private readonly RequestDelegate<ResourceContext> _requestDelegate;

        public ResourceSquid(RequestDelegate<ResourceContext> requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public static ResourceSquidBuilder Builder => new ResourceSquidBuilder();

        public async Task<Response> InvokeAsync(Request request)
        {
            var context = new ResourceContext
            {
                Request = request
            };

            await _requestDelegate(context);

            return context.Response;
        }

        public void Dispose() { }
    }
}