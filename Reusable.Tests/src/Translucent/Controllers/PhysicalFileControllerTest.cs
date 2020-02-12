using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Reusable.Translucent.Extensions;
using Xunit;

namespace Reusable.Translucent.Controllers
{
    public class PhysicalFileControllerTest : IClassFixture<TestHelperFixture>
    {
        private readonly TestHelperFixture _testHelper;


        public PhysicalFileControllerTest(TestHelperFixture testHelper)
        {
            _testHelper = testHelper;
        }

        [Fact]
        public async Task Can_handle_file_methods()
        {
            var resources =
                Resource
                    .Builder()
                    .Add(new PhysicalFileController(ControllerName.Empty))
                    .Register(TestHelper.CreateCache())
                    .Register(_testHelper.LoggerFactory)
                    .Build();

            var tempFileName = GetTempFileName();

            using (var file = await resources.GetFileAsync(tempFileName))
            {
                Assert.False(file.Exists());
            }

            await resources.WriteTextFileAsync(tempFileName, "Hi!");
            using (var file = await resources.GetFileAsync(tempFileName))
            {
                Assert.True(file.Exists());
                var value = await file.DeserializeTextAsync();
                Assert.Equal("Hi!", value);
            }

            await resources.DeleteFileAsync(tempFileName);
            using (var file = await resources.GetFileAsync(tempFileName))
            {
                Assert.False(file.Exists());
            }
        }

        [Fact]
        public async Task Can_read_and_write_text_file_from_share()
        {
            var resources =
                Resource
                    .Builder()
                    .Add(new PhysicalFileController(ControllerName.Empty))
                    .Register(TestHelper.CreateCache())
                    .Register(_testHelper.LoggerFactory)
                    .Build();
            
            var tempFileName = "//auth/name";

            using (var file = await resources.GetFileAsync(tempFileName))
            {
                Assert.False(file.Exists());
            }
        }

        private static string GetTempFileName() => Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".tmp");
    }
}