using System;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Translucent.Data;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace Reusable.Translucent.Middleware
{
    public class EnvironmentVariableMiddlewareTest
    {
        [Fact]
        public async Task Can_resolve_environment_variables()
        {
            Environment.SetEnvironmentVariable("TEST_VARIABLE", @"I:\test\this\path");

            var c = Mock.Create<TestFileController>(Behavior.CallOriginal, ControllerName.Empty);
            c.Arrange(x => x.Get(Arg.Matches<Request>(x => x.Uri.Path.Decoded.ToString().Equals(@"I:/test/this/path/test.txt")))).Returns(new Response().ToTask()).OccursOnce();
            
            var r = ResourceRepository.Create((x, s) => x.Add(c), (p, s) => p.UseMiddleware<EnvironmentVariableMiddleware>());
            
            await r.InvokeAsync(Request.CreateGet<FileRequest>(@"%TEST_VARIABLE%\test.txt"));

            c.Assert();
        }
    }
}