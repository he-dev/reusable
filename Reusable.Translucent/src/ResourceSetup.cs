using System;
using System.Collections.Generic;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Data;

namespace Reusable.Translucent
{
    public interface IResourceControllerSetup
    {
        IEnumerable<IResourceController> Controllers(IServiceProvider services);
    }
    
    public interface IResourceMiddlewareSetup
    {
        IEnumerable<IResourceController> Controllers(IServiceProvider services);
    }

    public interface IResourceSetup : IResourceControllerSetup
    {
        IEnumerable<IMiddlewareInfo> Middleware(IServiceProvider services);
    }

    public abstract class ResourceSetup : IResourceSetup
    {
        public abstract IEnumerable<IResourceController> Controllers(IServiceProvider services);

        public virtual IEnumerable<IMiddlewareInfo> Middleware(IServiceProvider services)
        {
            yield break;
        }

        protected static IMiddlewareInfo Use<T>(params object[] args) => MiddlewareInfo<ResourceContext>.Create<T>(args);
    }
}