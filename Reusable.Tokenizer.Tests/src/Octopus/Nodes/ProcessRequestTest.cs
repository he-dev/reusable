using System.Threading.Tasks;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Controllers;
using Reusable.Octopus.Data;
using Xunit;

namespace Reusable.Octopus.Nodes;

public class ProcessRequestTest
{
    private abstract class TestRequest : Request
    {
        public class Foo : TestRequest { }

        public class Bar : TestRequest { }

        public class Baz : TestRequest { }
    }

    private IResource Resource { get; } = new Resource.Builder
    {
        new ProcessRequest
        {
            Controllers =
            {
                new AdHocController<FileRequest>("file") { Name = "foo", Tags = { "a" }, OnRead = (c, r) => Response.Success(c.Name).ToTask() },
                new AdHocController<FileRequest>("file") { Name = "bar", Tags = { "b" }, OnRead = (c, r) => Response.Success(c.Name).ToTask() },
                new AdHocController<FileRequest>("file") { Name = "baz", Tags = { "c" }, OnRead = (c, r) => Response.Success(c.Name).ToTask() },
                new AdHocController<FileRequest>("file") { Name = "bax", Tags = { "b" }, OnRead = (c, r) => Response.Success(c.Name).ToTask() },
            }
        }
    }.Build();

    [Fact]
    public async Task Finds_resource_by_request_type()
    {
        using var response = await Resource.File("qux").ReadAsync();

        Assert.Equal(ResourceStatusCode.Success, response.StatusCode);
        Assert.Equal("baz", response.Body.Peek());
    }

    [Fact]
    public async Task Finds_resource_by_controller_name()
    {
        using var response = await Resource.ReadAsync<TestRequest.Bar>("qux", default, r => r.ControllerFilter = new ControllerFilterByName("bax"));

        Assert.Equal(ResourceStatusCode.Success, response.StatusCode);
        Assert.Equal("bax", response.Body.Peek());
    }

    [Fact]
    public async Task Does_not_find_resource_by_controller_name()
    {
        using var response = await Resource.ReadAsync<TestRequest.Bar>("qux", default, r => r.ControllerFilter = new ControllerFilterByName("bab"));

        Assert.Equal(ResourceStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Finds_resource_by_controller_tag()
    {
        using var response = await Resource.ReadAsync<TestRequest.Bar>("qux", default, r => r.ControllerFilter = new ControllerFilterByTag("b"));

        Assert.Equal(ResourceStatusCode.Success, response.StatusCode);
        Assert.Equal("bar", response.Body.Peek());
    }

    [Fact]
    public async Task Throws_when_CUD_methods_match_multiple_controllers()
    {
        await Assert.ThrowsAnyAsync<DynamicException>(async () => { await Resource.CreateAsync<TestRequest.Bar>("qux", default, r => r.ControllerFilter = new ControllerFilterByTag("b")); });
    }
}