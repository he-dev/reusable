using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Translucent.Data;

namespace Reusable.Translucent
{
    [PublicAPI]
    public interface IResource
    {
        Task<Response> InvokeAsync(Request request);
    }

    [PublicAPI]
    public class Resource : IResource //<TSetup> : IResourceRepository where TSetup : new()
    {
        private readonly RequestDelegate<ResourceContext> _requestDelegate;

        internal Resource(RequestDelegate<ResourceContext> requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public static ResourceBuilder Builder() => new ResourceBuilder();

        public static IResource From<TSetup>(IServiceProvider services) where TSetup : IResourceSetup, new()
        {
            var setup = new TSetup();

            var builder = Builder();

            foreach (var controller in setup.Controllers(services))
            {
                builder.Add(controller);
            }

            foreach (var (type, args) in setup.Middleware(services))
            {
                builder.Use(type, args);
            }

            return builder.Build(services);
        }

        public async Task<Response> InvokeAsync(Request request)
        {
            var context = new ResourceContext
            {
                Request = request
            };

            await _requestDelegate(context);

            return context.Response;
        }
    }
}