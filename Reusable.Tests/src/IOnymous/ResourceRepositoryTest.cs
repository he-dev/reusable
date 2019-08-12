using System.Threading.Tasks;
using Autofac;
using Xunit;

namespace Reusable.IOnymous
{
    public class ResourceRepositoryTest
    {
        [Fact]
        public async Task test()
        {
            var containerBuilder = new ContainerBuilder();
            containerBuilder.RegisterType<RelativeMiddleware>().WithParameter(new TypedParameter(typeof(UriString), new UriString(@"file:///Reusable/res/IOnymous")));
            var container = containerBuilder.Build();
            var lifetimeScope = container.BeginLifetimeScope();

            var pipelineBuilder = new PipelineBuilder(lifetimeScope);
            pipelineBuilder.Add<RelativeMiddleware>();
            var next = pipelineBuilder.Build<ResourceContext>();
            var resource = new ResourceRepository(new IResourceProvider[] { new EmbeddedFileProvider(typeof(ResourceRepositoryTest).Assembly) }, next);

            var file = await resource.InvokeAsync(new Request.Get("file:///test.txt"));
            
            Assert.True(file.Exists);
        }
    }
}