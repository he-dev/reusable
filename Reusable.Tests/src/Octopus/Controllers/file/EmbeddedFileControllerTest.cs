using System.Threading.Tasks;
using Reusable.Octopus.Data;
using Reusable.Octopus.Extensions;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Data;
using Xunit;

namespace Reusable.Octopus.Controllers;

public class EmbeddedFileControllerTest
{
    [Fact]
    public async Task Can_get_embedded_file()
    {
        var c = new EmbeddedFileController("Reusable", typeof(EmbeddedFileControllerTest).Assembly);
        using var response = await c.ReadAsync(Request.Read<FileRequest.Text>(@"res\octopus\test.txt"));

        Assert.True(response.Success());
        Assert.Equal("Hallo!", response.Body.Peek());
    }
}