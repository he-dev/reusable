using System;
using System.IO;
using System.Threading.Tasks;
using Reusable.IOnymous;
using Xunit;

namespace Reusable.Tests.XUnit.IOnymous
{
    public class PhysicalFileProviderTest
    {
        [Fact]
        public async Task Can_read_and_write_text_file()
        {
            var tempFileName = GetTempFileName();
            
            var provider = new PhysicalFileProvider();
            var file = await provider.GetFileAsync(tempFileName, MimeType.Text);
            
            Assert.False(file.Exists);

            file = await provider.WriteTextFileAsync(tempFileName, "Hi!");
            
            Assert.True(file.Exists);

            var value = await file.DeserializeTextAsync();
            
            Assert.Equal("Hi!", value);

            file = await provider.DeleteFileAsync(tempFileName);
            
            Assert.False(file.Exists);
        }
        
        private static string GetTempFileName() => Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".tmp");
    }
}