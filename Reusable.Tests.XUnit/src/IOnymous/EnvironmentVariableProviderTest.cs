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
            var file = await provider.GetFileInfoAsync("%test-variable-1%/test.txt");
            
            Assert.False(file.Exists);
            Assert.Equal("c:/temp/test.txt", file.Uri.Path);
        }
    }
}