using System;
using System.Threading.Tasks;
using Reusable.Octopus.Data;
using Reusable.Translucent.Data;
using Xunit;

namespace Reusable.Octopus.Nodes;

public class EnvironmentVariableMiddlewareTest
{
    [Fact]
    public async Task Can_resolve_environment_variables()
    {
        Environment.SetEnvironmentVariable("TEST_VARIABLE", @"X:\test\this");

        var controller = new ExpandEnvironmentVariables();
        var context = new ResourceContext(Request.Read<FileRequest.Text>(@"%TEST_VARIABLE%\file.txt"));
        await controller.InvokeAsync(context);

        Assert.Equal(@"X:\test\this\file.txt", context.Request.ResourceName.Peek());
    }
}