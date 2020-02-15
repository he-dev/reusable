using System;
using System.Collections.Generic;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public delegate IEnumerable<CreateControllerDelegate> ControllerFactory(IServiceProvider services);

    public delegate IEnumerable<CreateMiddlewareDelegate> MiddlewareFactory(IServiceProvider services);

    public delegate IController CreateControllerDelegate();

    public delegate IMiddleware CreateMiddlewareDelegate(RequestDelegate<ResourceContext> next);

    public static class CreateControllerHelper
    {
        public static IEnumerable<CreateControllerDelegate> Create(params CreateControllerDelegate[] factories)
        {
            return factories;
        }
    }
}