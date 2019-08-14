using System.Threading.Tasks;
using Xunit;

namespace Reusable.IOnymous.Translucent
{
    public class ResourceRepositoryTest
    {
        [Fact]
        public async Task test()
        {
            var resources = ResourceSquid.Builder.AddController(new EmbeddedFileController<ResourceRepositoryTest>(@"Reusable/res/IOnymous")).Build();

            var file = await resources.InvokeAsync(new Request.Get("file:///test.txt"));

            Assert.True(file.Exists);
        }
    }
}