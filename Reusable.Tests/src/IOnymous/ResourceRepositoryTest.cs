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
//            var containerBuilder = new ContainerBuilder();
//            containerBuilder.RegisterType<RelativeMiddleware>().WithParameter(new TypedParameter(typeof(UriString), new UriString(@"file:///Reusable/res/IOnymous")));
//            var container = containerBuilder.Build();
//            var lifetimeScope = container.BeginLifetimeScope();

            var middlewareBuilder = new MiddlewareBuilder();
            //middlewareBuilder.Use<BaseUriMiddleware>(new UriString(@"file:///Reusable/res/IOnymous"));
            var next = middlewareBuilder.Build<ResourceContext>();
            var resource = new ResourceRepository(new IResourceProvider[] { new EmbeddedFileProvider(typeof(ResourceRepositoryTest).Assembly, @"Reusable/res/IOnymous") }, next);

            var file = await resource.InvokeAsync(new Request.Get("file:///test.txt"));
            
            Assert.True(file.Exists);
        }
    }
}