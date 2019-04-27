using System;
using System.Threading.Tasks;
using Reusable.IOnymous;
using Xunit;

namespace Reusable.Tests.XUnit.IOnymous
{
    public class EnvironmentVariableProviderTest
    {
        [Fact]
        public async Task Can_resolve_environment_variables()
        {
            Environment.SetEnvironmentVariable("test-variable-1", @"c:\temp");

            var provider = new PhysicalFileProvider().DecorateWith(EnvironmentVariableProvider.Factory());
            //var provider = new PhysicalFileProvider() + EnvironmentVariableProvider.Factory();
            //var provider = new PhysicalFileProvider() + (left =>  new EnvironmentVariableProvider(left));
            var file = await provider.GetFileAsync(@"%test-variable-1%\test.txt", MimeType.Text);
            
            Assert.False(file.Exists);
            Assert.Equal("c:/temp/test.txt", file.Uri.Path.Decoded);
        }
    }
}