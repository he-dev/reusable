using System;
using System.Threading.Tasks;
using Reusable.Extensions;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace Reusable.Translucent.Middleware
{
    public class EnvironmentVariableMiddlewareTest : IClassFixture<TestHelperFixture>
    {
        private readonly TestHelperFixture _testHelper;

        public EnvironmentVariableMiddlewareTest(TestHelperFixture testHelper)
        {
            _testHelper = testHelper;
        }

        [Fact]
        public async Task Can_resolve_environment_variables()
        {
            Environment.SetEnvironmentVariable("TEST_VARIABLE", @"I:\test\this\path");

            var c = Mock.Create<TestFileController>(Behavior.CallOriginal);
            c.Arrange(x => x.ReadAsync(Arg.Matches<FileRequest>(y => y.ResourceName.Equals(@"I:\test\this\path\test.txt")))).Returns(new Response().ToTask()).OccursOnce();

            var r = new Resource(new IResourceMiddleware[]
            {
                new EnvironmentVariableResourceMiddleware(), 
                new ResourceSearch(new[] { c }),
            });

            await r.InvokeAsync(Request.Read<FileRequest>(@"%TEST_VARIABLE%\test.txt"));

            c.Assert();
        }
    }
}