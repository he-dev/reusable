using System.Threading.Tasks;
using Reusable.Translucent.Controllers;
using Xunit;

namespace Reusable.Translucent
{
    public class ResourceSquidTest
    {
        [Fact]
        public async Task test()
        {
            var resources = ResourceSquid.Builder.UseController(new EmbeddedFileController<ResourceSquidTest>(@"Reusable/res/IOnymous")).Build();

            var file = await resources.InvokeAsync(new Request.Get("file:///test.txt"));

            Assert.True(file.Exists());
        }
    }
}