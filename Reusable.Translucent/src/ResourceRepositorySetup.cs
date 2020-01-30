using System;
using System.Collections.Generic;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public interface IResourceRepositorySetup
    {
        IEnumerable<IResourceController> Controllers(IServiceProvider services);

        IEnumerable<IMiddlewareInfo<ResourceContext>> Middleware(IServiceProvider services);
    }
    
    public abstract class ResourceRepositorySetup : IResourceRepositorySetup
    {
        public abstract IEnumerable<IResourceController> Controllers(IServiceProvider services);

        public virtual IEnumerable<IMiddlewareInfo<ResourceContext>> Middleware(IServiceProvider services)
        {
            yield break;
        }

        protected static IMiddlewareInfo<ResourceContext> Use<T>(params object[] args) => MiddlewareInfo<ResourceContext>.Create<T>(args);
    }
}