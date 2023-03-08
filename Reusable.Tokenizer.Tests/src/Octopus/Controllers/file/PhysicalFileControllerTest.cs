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
        new ManageString(),
        new ProcessRequest
        {
            Controllers = { new PhysicalFileController() }
        }
    }.Build();

    [Fact]
    public async Task Can_handle_file_methods()
    {
        var tempFileName = GetTempFileName();

        using (var response = await Resource.Read().File(tempFileName).InvokeAsync())
        {
            Assert.True(response.NotFound());
        }

        using (await Resource.Create().File(tempFileName).Data("Hi!").InvokeAsync())
        using (var response = await Resource.File(tempFileName).As(typeof(string)).ReadAsync())
        {
            Assert.True(response.Success());
            Assert.Equal("Hi!", response.Body.Value);
        }

        using (await Resource.File(tempFileName).DeleteAsync())
        using (var response = await Resource.File(tempFileName).ReadAsync())
        {
            Assert.True(response.NotFound());
        }

        using (var response = await Resource.File(tempFileName).ReadAsync())
        {
            Assert.True(response.NotFound());
        }
    }

    [Fact]
    public async Task Can_read_and_write_text_file_from_share()
    {
        var tempFileName = "//auth/name";

        using var response = await Resource.File(tempFileName).ReadAsync();
        Assert.False(response.Success());
    }

    private static string GetTempFileName() => Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".tmp");
}