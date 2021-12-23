using System.Threading.Tasks;
using Reusable.Translucent.Data;
using Reusable.Translucent.Extensions;
using Xunit;

namespace Reusable.Translucent.Controllers
{
    public class EmbeddedFileControllerTest
    {
        [Fact]
        public async Task Can_get_embedded_file()
        {
            var c = new EmbeddedFileController("Reusable", typeof(EmbeddedFileControllerTest).Assembly);
            using var file = await c.ReadAsync(Request.Read<FileRequest.Text>(@"res\translucent\test.txt"));

            Assert.True(file.Exists());
            Assert.Equal("Hallo!", file.Body.Peek());
        }

//        [Fact]
//        public async Task Throws_when_scheme_not_file()
//        {
//            var provider = new EmbeddedFileProvider(typeof(EmbeddedFileProviderTest).Assembly);
//
//            await Assert.ThrowsAnyAsync<DynamicException>(async () => await provider.GetAsync("res/ionymous/test.txt"));
//        }
    }
}