using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Reusable.Translucent.config.Controllers
{
    public class PhysicalFileControllerTest
    {
        private static readonly IResourceRepository Resources = ResourceRepository.Create(c => c.AddPhysicalFiles());

        [Fact]
        public async Task Can_read_and_write_text_file()
        {
            var tempFileName = GetTempFileName();

            using (var file = await Resources.GetFileAsync(tempFileName))
            {
                Assert.False(file.Exists());
            }

            await Resources.WriteTextFileAsync(tempFileName, "Hi!");
            using (var file = await Resources.GetFileAsync(tempFileName))
            {
                Assert.True(file.Exists());
                var value = await file.DeserializeTextAsync();
                Assert.Equal("Hi!", value);
            }


            await Resources.DeleteFileAsync(tempFileName);
            using (var file = await Resources.GetFileAsync(tempFileName))
            {
                Assert.False(file.Exists());
            }
        }

        [Fact]
        public async Task Can_read_and_write_text_file_from_share()
        {
            var tempFileName = "//auth/name";

            using (var file = await Resources.GetFileAsync(tempFileName))
            {
                Assert.False(file.Exists());
            }
        }

        private static string GetTempFileName() => Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".tmp");
    }
}