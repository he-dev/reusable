using System;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Translucent
{
    [PublicAPI]
    public interface IResourceRepository
    {
        Task<Response> InvokeAsync(Request request);
    }

    [PublicAPI]
    public class ResourceRepository<TSetup> : IResourceRepository where TSetup : new()
    {
        private readonly RequestDelegate<ResourceContext> _requestDelegate;

        public ResourceRepository(IServiceProvider serviceProvider)
        {
            _requestDelegate = ResourceRepositoryBuilder.Empty.UseSetup<TSetup>().Build(serviceProvider);
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

    public static class ResourceRepository
    {
        public static IResourceRepository Create(ConfigureResourcesDelegate configureResources, ConfigurePipelineDelegate<ResourceContext>? configurePipeline = default)
        {
            return new ResourceRepository<QuickSetup<ResourceContext>>(ImmutableServiceProvider.Empty.Add(configureResources).Add(configurePipeline));
        }
    }

    [PublicAPI]
    public class QuickSetup<T>
    {
        public void ConfigureResources(IResourceCollection resources, IServiceProvider serviceProvider, ConfigureResourcesDelegate configure)
        {
            configure(resources, serviceProvider);
        }

        public void ConfigurePipeline(IPipelineBuilder<T> repository, IServiceProvider serviceProvider, ConfigurePipelineDelegate<T>? configure)
        {
            configure?.Invoke(repository, serviceProvider);
        }
    }

    public delegate void ConfigureResourcesDelegate(IResourceCollection resource, IServiceProvider serviceProvider);

    public delegate void ConfigurePipelineDelegate<out T>(IPipelineBuilder<T> pipelineController, IServiceProvider serviceProvider);
}