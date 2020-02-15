using System;
using System.Collections.Generic;
using Reusable.Translucent.Abstractions;

namespace Reusable.Translucent
{
    public delegate IEnumerable<CreateControllerDelegate> ControllerFactory(IServiceProvider services);

    public delegate IEnumerable<CreateMiddlewareDelegate> MiddlewareFactory(IServiceProvider services);

    public delegate IResourceController CreateControllerDelegate();

    public delegate IResourceMiddleware CreateMiddlewareDelegate(RequestDelegate<ResourceContext> next);

    public static class CreateControllerHelper
    {
        public static IEnumerable<CreateControllerDelegate> Create(params CreateControllerDelegate[] factories)
        {
            return factories;
        }
    }
}