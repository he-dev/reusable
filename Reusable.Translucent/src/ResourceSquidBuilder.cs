using System;
using System.Collections.Generic;

namespace Reusable.Translucent
{
    public class ResourceSquidBuilder
    {
        private readonly List<IResourceController> _controllers = new List<IResourceController>();
        private readonly RequestCallbackBuilder _requestCallbackBuilder = new RequestCallbackBuilder();

        public ResourceSquidBuilder ConfigureMiddleware(Action<RequestCallbackBuilder> configureMiddleware)
        {
            configureMiddleware(_requestCallbackBuilder);
            return this;
        }

        public ResourceSquidBuilder UseController(IResourceController controller)
        {
            _controllers.Add(controller);
            return this;
        }

        public ResourceSquid Build()
        {
            var requestCallback = _requestCallbackBuilder.UseControllers(_controllers).Build<ResourceContext>();
            return new ResourceSquid(requestCallback);
        }
    }
}