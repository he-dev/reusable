using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Reusable.Translucent.Extensions;
using Xunit;

namespace Reusable.Translucent.Controllers
{
    public class PhysicalFileControllerTest // : IClassFixture<TestHelperFixture>
    {
        private static readonly IResource Resource = new Resource(new[] { new PhysicalFileResourceController() });

        // public PhysicalFileControllerTest(TestHelperFixture testHelper)
        // {
        //     _resource = new Resource(new[] { new PhysicalFileResourceController() });
        // }

        [Fact]
        public async Task Can_handle_file_methods()
        {
            var tempFileName = GetTempFileName();

            using (var file = await Resource.GetFileAsync(tempFileName))
            {
                Assert.False(file.Exists());
            }

            await Resource.WriteTextFileAsync(tempFileName, "Hi!");
            using (var file = await Resource.GetFileAsync(tempFileName))
            {
                Assert.True(file.Exists());
                var value = await file.DeserializeTextAsync();
                Assert.Equal("Hi!", value);
            }

            await Resource.DeleteFileAsync(tempFileName);
            using (var file = await Resource.GetFileAsync(tempFileName))
            {
                Assert.False(file.Exists());
            }
        }

        [Fact]
        public async Task Can_read_and_write_text_file_from_share()
        {
            var tempFileName = "//auth/name";

            using var file = await Resource.GetFileAsync(tempFileName);
            Assert.False(file.Exists());
        }

        private static string GetTempFileName() => Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".tmp");
    }
}