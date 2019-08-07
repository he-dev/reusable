using System.Threading.Tasks;
using Xunit;

namespace Reusable.IOnymous
{
    public class EmbeddedFileProviderTest
    {
        [Fact]
        public async Task Can_get_embedded_file()
        {
            var provider = new EmbeddedFileProvider(typeof(EmbeddedFileProviderTest).Assembly);

            var file = await provider.GetFileAsync("res/ionymous/test.txt", MimeType.Plain);

            Assert.True(file.Exists);
            Assert.Equal("Hallo!", await file.DeserializeTextAsync());
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