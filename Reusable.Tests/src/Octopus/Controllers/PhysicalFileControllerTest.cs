using System;
using System.IO;
using System.Threading.Tasks;
using Reusable.Essentials.Extensions;
using Reusable.Octopus;
using Reusable.Octopus.Extensions;
using Reusable.Octopus.Nodes;
using Reusable.Translucent.Data;
using Xunit;

namespace Reusable.Translucent.Controllers
{
    public class PhysicalFileControllerTest // : IClassFixture<TestHelperFixture>
    {
        private static readonly IResource Resource = new Resource.Builder
        {
            new ProcessRequest
            {
                Controllers = { new PhysicalFileController() }
            }
        }.Build();

        // public PhysicalFileControllerTest(TestHelperFixture testHelper)
        // {
        //     _resource = new Resource(new[] { new PhysicalFileResourceController() });
        // }

        [Fact]
        public async Task Can_handle_file_methods()
        {
            var tempFileName = GetTempFileName();

            using (var response = await Resource.ReadAsync<FileRequest.Text>(tempFileName))
            {
                Assert.True(response.NotFound());
            }

            await Resource.WriteTextFileAsync(tempFileName, "Hi!");
            using (var response = await Resource.ReadAsync<FileRequest.Stream>(tempFileName))
            {
                Assert.True(response.Success());
                var value = await ((Stream)response.Body.Peek()).ReadTextAsync();
                Assert.Equal("Hi!", value);
            }

            await Resource.DeleteFileAsync(tempFileName);
            using (var response = await Resource.ReadAsync<FileRequest.Text>(tempFileName))
            {
                Assert.True(response.NotFound());
            }
            
            using (var response = await Resource.ReadAsync<FileRequest.Text>(tempFileName))
            {
                Assert.True(response.NotFound());
            }
        }

        [Fact]
        public async Task Can_read_and_write_text_file_from_share()
        {
            var tempFileName = "//auth/name";

            using var response = await Resource.ReadAsync<FileRequest.Text>(tempFileName);
            Assert.False(response.Success());
        }

        private static string GetTempFileName() => Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".tmp");
    }
}