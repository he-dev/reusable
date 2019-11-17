using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Reusable.Translucent.Controllers;


namespace Reusable.Translucent
{
    public interface IResourceRepository : IDisposable
    {
        Task<Response> InvokeAsync(Request request);
    }

    public class ResourceRepository : IResourceRepository
    {
        private readonly RequestDelegate<ResourceContext> _requestDelegate;

        public ResourceRepository(RequestDelegate<ResourceContext> requestDelegate)
        {
            _requestDelegate = requestDelegate;
        }

        public static IResourceRepository Create(ConfigureResourcesDelegate configureResources, ConfigurePipelineDelegate<ResourceContext>? configurePipeline = default)
        {
            return
                ResourceRepositoryBuilder
                    .Empty
                    .UseSetup<QuickSetup<ResourceContext>>()
                    .Build(
                        ImmutableServiceProvider
                            .Empty
                            .Add(configureResources)
                            .Add(configurePipeline));
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

        public void Dispose() { }
    }


    [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
    public class QuickSetup<T>
    {
        public void ConfigureResources(IResourceCollection resources, ConfigureResourcesDelegate configure)
        {
            configure(resources);
        }

        public void ConfigurePipeline(IPipelineBuilder<T> repository, ConfigurePipelineDelegate<T>? configure)
        {
            configure?.Invoke(repository);
        }
    }

    public delegate void ConfigureResourcesDelegate(IResourceCollection resource);

    public delegate void ConfigurePipelineDelegate<out T>(IPipelineBuilder<T> pipelineController);
}