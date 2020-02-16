using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Reusable.Exceptionize;
using Reusable.OmniLog;
using Reusable.OmniLog.Abstractions;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;
using Reusable.Translucent.Middleware;

namespace Reusable.Translucent
{
    [PublicAPI]
    public interface IResource
    {
        Task<Response> InvokeAsync(Request request);
    }

    [PublicAPI]
    public class Resource : IResource
    {
        private readonly IResourceMiddleware _resourceMiddleware;

        public Resource(IResourceMiddleware resourceMiddleware) => _resourceMiddleware = resourceMiddleware;

        public static ResourceBuilder Builder() => new ResourceBuilder();

        public async Task<Response> InvokeAsync(Request request)
        {
            var context = new ResourceContext {Request = request};
            await _resourceMiddleware.InvokeAsync(context);
            return context.Response;
        }
    }
}