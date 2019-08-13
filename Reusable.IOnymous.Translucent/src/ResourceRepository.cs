using System;
using System.Threading.Tasks;

namespace Reusable.IOnymous
{
    public interface IResourceRepository : IDisposable
    {
        Task<IResource> InvokeAsync(Request request);
    }

    public class ResourceRepository : IResourceRepository
    {
        private readonly RequestCallback<ResourceContext> _requestCallback;

        public ResourceRepository(RequestCallback<ResourceContext> requestCallback)
        {
            _requestCallback = requestCallback;
        }

        public ResourceRepository(Action<MiddlewareBuilder> middleware)
            : this(new Func<RequestCallback<ResourceContext>>(() =>
            {
                var builder = new MiddlewareBuilder();
                middleware(builder);
                return builder.Build<ResourceContext>();
            })()) { }


        public async Task<IResource> InvokeAsync(Request request)
        {
            var context = new ResourceContext
            {
                Request = request
            };

            await _requestCallback(context);

            return context.Response;
        }

        public void Dispose()
        {
            
        }
    }

    public class ResourceContext
    {
        public Request Request { get; set; }

        public IResource Response { get; set; }
    }
}