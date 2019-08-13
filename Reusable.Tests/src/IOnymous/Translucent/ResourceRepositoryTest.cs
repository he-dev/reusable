using System.Threading.Tasks;
using Autofac;
using Reusable.Data;
using Xunit;

namespace Reusable.IOnymous
{
    public class ResourceRepositoryTest
    {
        [Fact]
        public async Task test()
        {
            var resources = new ResourceRepository(builder => { builder.UseResources(new EmbeddedFileProvider<ResourceRepositoryTest>(@"Reusable/res/IOnymous")); });

            var file = await resources.InvokeAsync(new Request.Get("file:///test.txt"));

            Assert.True(file.Exists);
        }
    }
}