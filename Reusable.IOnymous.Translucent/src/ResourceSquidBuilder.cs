using System;
using System.Collections.Generic;

namespace Reusable.IOnymous
{
    public class ResourceSquidBuilder
    {
        private readonly List<IResourceController> _controllers = new List<IResourceController>();
        private readonly MiddlewareBuilder _middlewareBuilder = new MiddlewareBuilder();

        public ResourceSquidBuilder ConfigureMiddleware(Action<MiddlewareBuilder> configureMiddleware)
        {
            configureMiddleware(_middlewareBuilder);
            return this;
        }

        public ResourceSquidBuilder AddController(IResourceController controller)
        {
            _controllers.Add(controller);
            return this;
        }

        public ResourceSquid Build()
        {
            var requestCallback = _middlewareBuilder.UseControllers(_controllers).Build<ResourceContext>();
            return new ResourceSquid(requestCallback);
        }
    }
}