using System.Threading.Tasks;

namespace Reusable.IOnymous
{
    public class ResourceRepository
    {
        private readonly RequestCallback<ResourceContext> _requestCallback;

        public ResourceRepository(RequestCallback<ResourceContext> requestCallback)
        {
            _requestCallback = requestCallback;
        }

        public async Task<IResource> InvokeAsync(Request request)
        {
            var context = new ResourceContext
            {
                Request = request
            };

            await _requestCallback(context);

            return context.Response;
        }
    }

    public class ResourceContext
    {
        public Request Request { get; set; }

        public IResource Response { get; set; }
    }
}