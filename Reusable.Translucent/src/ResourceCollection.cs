using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Middleware;

namespace Reusable.Translucent
{
    public interface IResourceCollection : IList<IResourceController>
    {
        IResourceCollection Add<T>(T controller) where T : IResourceController;
    }

    public class ResourceCollection : List<IResourceController>, IResourceCollection
    {
        public IResourceCollection Add<T>(T controller) where T : IResourceController
        {
            base.Add(controller);
            return this;
        }
    }
}