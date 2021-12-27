using System;
using System.IO;
using System.Threading.Tasks;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Data;
using Reusable.Octopus.Extensions;
using Reusable.Octopus.Nodes;
using Xunit;

namespace Reusable.Octopus.Controllers.file;

public class PhysicalFileControllerTest
{
    private static readonly IResource Resource = new Resource.Builder
    {
        new ProcessRequest
        {
            Controllers = { new PhysicalFileController() }
        }
    }.Build();
    
    [Fact]
    public async Task Can_handle_file_methods()
    {
        var tempFileName = GetTempFileName();

        using (var response = await Resource.ReadAsync<FileRequest.Text>(tempFileName))
        {
            Assert.True(response.NotFound());
        }

        await Resource.WriteFileAsync(tempFileName, "Hi!");
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