using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Reusable.Exceptionize;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Middleware;

namespace Reusable.Translucent
{
    public class ResourceBuilder
    {
        private readonly List<IResourceController> _controller = new List<IResourceController>();
        private readonly List<IResourceMiddleware> _middleware = new List<IResourceMiddleware>();

        public ResourceBuilder UseController(IResourceController controller)
        {
            _controller.Add(controller);
            return this;
        }

        public ResourceBuilder UseMiddleware(IResourceMiddleware middleware)
        {
            _middleware.Add(middleware);
            return this;
        }

        public IResource Build() => new Resource(_middleware.Append(new ResourceSearch(_controller)).Chain().Head());
    }
}