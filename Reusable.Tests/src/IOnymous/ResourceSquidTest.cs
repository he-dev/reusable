using System.Threading.Tasks;
using Reusable.IOnymous.Controllers;
using Xunit;

namespace Reusable.IOnymous
{
    public class ResourceSquidTest
    {
        [Fact]
        public async Task test()
        {
            var resources = ResourceSquid.Builder.AddController(new EmbeddedFileController<ResourceSquidTest>(@"Reusable/res/IOnymous")).Build();

            var file = await resources.InvokeAsync(new Request.Get("file:///test.txt"));

            Assert.True(file.Exists);
        }
    }
}