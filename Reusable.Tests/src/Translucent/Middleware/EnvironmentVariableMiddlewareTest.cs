using System;
using System.Threading.Tasks;
using Reusable.Extensions;
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

            var c = Mock.Create<TestFileController>(Behavior.CallOriginal, ControllerName.Empty);
            c.Arrange(x => x.Get(Arg.Matches<Request>(y => y.ResourceName.Equals(@"I:\test\this\path\test.txt")))).Returns(new Response().ToTask()).OccursOnce();

            var r =
                Resource
                    .Builder()
                    .UseController(c)
                    .UseMiddleware(_ => next => new EnvironmentVariableMiddleware(next))
                    .Build(ImmutableServiceProvider.Empty.Add(_testHelper.Cache).Add(_testHelper.LoggerFactory));

            await r.InvokeAsync(Request.CreateGet<FileRequest>(@"%TEST_VARIABLE%\test.txt"));

            c.Assert();
        }
    }
}