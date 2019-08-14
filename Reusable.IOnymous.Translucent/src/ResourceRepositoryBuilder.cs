using System;
using System.Collections.Generic;

namespace Reusable.IOnymous
{
    public class ResourceRepositoryBuilder
    {
        private readonly List<IResourceProvider> _resourceProviders = new List<IResourceProvider>();
        private readonly MiddlewareBuilder _middlewareBuilder = new MiddlewareBuilder();

        public ResourceRepositoryBuilder Middleware(Action<MiddlewareBuilder> buildMiddleware)
        {
            buildMiddleware(_middlewareBuilder);
            return this;
        }

        public ResourceRepositoryBuilder Resources(params IResourceProvider[] resourceProviders)
        {
            _resourceProviders.AddRange(resourceProviders);
            return this;
        }

        public ResourceRepository Build()
        {
            var requestCallback = _middlewareBuilder.UseResources(_resourceProviders).Build<ResourceContext>();
            return new ResourceRepository(requestCallback);
        }
    }
}