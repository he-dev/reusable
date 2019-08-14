using System;
using System.Collections;
using System.Collections.Generic;
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

        public static ResourceRepositoryBuilder Builder => new ResourceRepositoryBuilder();
        
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

    public class ResourceRepositoryBuilder
    {
        private readonly IList<IResourceProvider> _resourceProviders = new List<IResourceProvider>();
        private readonly MiddlewareBuilder _middlewareBuilder = new MiddlewareBuilder();

        public ResourceRepositoryBuilder Middleware(Action<MiddlewareBuilder> buildMiddleware)
        {
            buildMiddleware(_middlewareBuilder);
            return this;
        }

        public ResourceRepositoryBuilder AddResourceProvider(IResourceProvider resourceProvider)
        {
            _resourceProviders.Add(resourceProvider);
            return this;
        }

        public ResourceRepository Build()
        {
            var requestCallback = _middlewareBuilder.UseResources(_resourceProviders).Build<ResourceContext>();
            return new ResourceRepository(requestCallback);
        }
    }

    public class ResourceContext
    {
        public Request Request { get; set; }

        public IResource Response { get; set; }
    }
}