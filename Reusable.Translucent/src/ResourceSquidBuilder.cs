using System;
using System.Collections.Generic;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public class ResourceSquidBuilder
    {
        private readonly List<IResourceController> _controllers = new List<IResourceController>();
        private readonly RequestDelegateBuilder _requestDelegateBuilder = new RequestDelegateBuilder();

        public ResourceSquidBuilder ConfigureMiddleware(Action<RequestDelegateBuilder> configureMiddleware)
        {
            configureMiddleware(_requestDelegateBuilder);
            return this;
        }

        public ResourceSquidBuilder UseController(IResourceController controller)
        {
            _controllers.Add(controller);
            return this;
        }

        public ResourceSquid Build()
        {
            var requestCallback = _requestDelegateBuilder.UseControllers(_controllers).Build<ResourceContext>();
            return new ResourceSquid(requestCallback);
        }
    }
}