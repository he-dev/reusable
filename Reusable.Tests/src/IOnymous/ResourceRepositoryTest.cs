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
            middlewareBuilder.UseResources(new IResourceProvider[] { new EmbeddedFileProvider<ResourceRepositoryTest>(@"Reusable/res/IOnymous") });
            var middleware = middlewareBuilder.Build<ResourceContext>();
            var resource = new ResourceRepository(middleware);

            var file = await resource.InvokeAsync(new Request.Get("file:///test.txt"));
            
            Assert.True(file.Exists);
        }
    }
}