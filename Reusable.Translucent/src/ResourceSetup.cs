using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.Translucent.Abstractions;

namespace Reusable.Translucent
{
    public delegate Task RequestDelegate(ResourceContext context);
    
    public delegate IResourceController CreateControllerDelegate();

    public delegate IResourceMiddleware CreateMiddlewareDelegate(RequestDelegate next);
}