using System.Threading.Tasks;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Annotations;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Data;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace Reusable.Translucent.Middleware
{
    public class ResourceMiddlewareTest : IClassFixture<TestHelperFixture>
    {
        private readonly TestHelperFixture _testHelper;

        public ResourceMiddlewareTest(TestHelperFixture testHelper)
        {
            _testHelper = testHelper;
        }

        [Fact]
        public async Task Gets_first_matching_resource_by_default()
        {
            var c1 = Mock.Create<TestFileController>(Behavior.CallOriginal);
            var c2 = Mock.Create<TestFileController>(Behavior.CallOriginal);
            var c3 = Mock.Create<TestFileController>(Behavior.CallOriginal);

            Mock.Arrange(() => c1.ReadAsync(Arg.IsAny<FileRequest>())).Returns(new Response { StatusCode = ResourceStatusCode.NotFound }.ToTask()).OccursOnce();
            Mock.Arrange(() => c2.ReadAsync(Arg.IsAny<FileRequest>())).Returns(new Response { StatusCode = ResourceStatusCode.Success }.ToTask()).OccursOnce();
            Mock.Arrange(() => c3.ReadAsync(Arg.IsAny<FileRequest>())).OccursNever();

            var resources =
                Resource
                    .Builder()
                    .UseController(c1)
                    .UseController(c2)
                    .UseController(c3)
                    .Build(ImmutableServiceProvider.Empty.Add(TestHelper.CreateCache()).Add(TestHelper.CreateLoggerFactory()));

            await resources.InvokeAsync(Request.Read<FileRequest>("file:///foo"));

            c1.Assert();
            c2.Assert();
            c3.Assert();
        }

        [Fact]
        public async Task Can_filter_controllers_by_controller_id()
        {
            var c1 = Mock.Create<TestFileController>(Behavior.CallOriginal).Pipe(x => x.Name = "a");
            var c2 = Mock.Create<TestFileController>(Behavior.CallOriginal).Pipe(x => x.Name = "b");
            var c3 = Mock.Create<TestFileController>(Behavior.CallOriginal).Pipe(x => x.Name = "d");

            Mock.Arrange(() => c1.ReadAsync(Arg.IsAny<FileRequest>())).OccursNever();
            Mock.Arrange(() => c2.ReadAsync(Arg.IsAny<FileRequest>())).Returns(new Response { StatusCode = ResourceStatusCode.Success }.ToTask()).OccursOnce();
            Mock.Arrange(() => c3.ReadAsync(Arg.IsAny<FileRequest>())).OccursNever();

            var resources =
                Resource
                    .Builder()
                    .UseController(c1)
                    .UseController(c2)
                    .UseController(c3)
                    .Build(ImmutableServiceProvider.Empty.Add(_testHelper.Cache).Add(_testHelper.LoggerFactory));

            await resources.InvokeAsync(Request.Read<FileRequest>("file:///foo").Pipe(r => { r.ControllerName = "b"; }));

            c1.Assert();
            c2.Assert();
            c3.Assert();
        }

        [Fact]
        public async Task Can_filter_controllers_by_request()
        {
            var c1 = Mock.Create<TestFileController>(Behavior.CallOriginal).Pipe(x => x.Name = "a");
            var c2 = Mock.Create<TestHttpController>(Behavior.CallOriginal).Pipe(x => x.Name = "b");
            var c3 = Mock.Create<TestFileController>(Behavior.CallOriginal).Pipe(x => x.Name = "d");

            Mock.Arrange(() => c1.ReadAsync(Arg.IsAny<FileRequest>())).OccursNever();
            Mock.Arrange(() => c2.ReadAsync(Arg.IsAny<HttpRequest>())).Returns(new Response { StatusCode = ResourceStatusCode.Success }.ToTask()).OccursOnce();
            Mock.Arrange(() => c3.ReadAsync(Arg.IsAny<FileRequest>())).OccursNever();

            var resources =
                Resource
                    .Builder()
                    .UseController(c1)
                    .UseController(c2)
                    .UseController(c3)
                    .Build(ImmutableServiceProvider.Empty.Add(_testHelper.Cache).Add(_testHelper.LoggerFactory));

            await resources.InvokeAsync(Request.Read<HttpRequest>("///foo"));

            c1.Assert();
            c2.Assert();
            c3.Assert();
        }

        [Fact]
        public async Task Can_filter_controllers_by_tag()
        {
            var c1 = Mock.Create<TestFileController>(Behavior.CallOriginal).Pipe(x => x.Name = "a");
            var c2 = Mock.Create<TestFileController>(Behavior.CallOriginal).Pipe(x =>
            {
                x.Name = "b";
                x.Tags.Add("bb");
            });

            Mock.Arrange(() => c1.ReadAsync(Arg.IsAny<FileRequest>())).CallOriginal().OccursNever();
            Mock.Arrange(() => c2.ReadAsync(Arg.IsAny<FileRequest>())).CallOriginal().OccursOnce();

            var resources =
                Resource
                    .Builder()
                    .UseController(c1)
                    .UseController(c2)
                    .Build(ImmutableServiceProvider.Empty.Add(TestHelper.CreateCache()).Add(TestHelper.CreateLoggerFactory()));

            await resources.InvokeAsync(Request.Read<FileRequest>("file:///foo").Pipe(r => { r.ControllerTags.Add("bb"); }));

            c1.Assert();
            c2.Assert();
        }

        [Fact]
        public async Task Throws_when_UPDATE_matches_multiple_controllers()
        {
            var c1 = Mock.Create<TestFileController>(Behavior.CallOriginal);
            var c2 = Mock.Create<TestFileController>(Behavior.CallOriginal);
            var c3 = Mock.Create<TestFileController>(Behavior.CallOriginal);

            Mock.Arrange(() => c1.ReadAsync(Arg.IsAny<FileRequest>())).OccursNever();
            Mock.Arrange(() => c2.ReadAsync(Arg.IsAny<FileRequest>())).OccursNever();
            Mock.Arrange(() => c3.ReadAsync(Arg.IsAny<FileRequest>())).OccursNever();

            var resources =
                Resource
                    .Builder()
                    .UseController(c1)
                    .UseController(c2)
                    .UseController(c3)
                    .Build(ImmutableServiceProvider.Empty.Add(TestHelper.CreateCache()).Add(TestHelper.CreateLoggerFactory()));

            await Assert.ThrowsAnyAsync<DynamicException>(async () => await resources.InvokeAsync(Request.Update<FileRequest>("file:///foo")));

            c1.Assert();
            c2.Assert();
            c3.Assert();
        }
    }

    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class TestFileController : ResourceController<FileRequest>
    {
        public override Task<Response> ReadAsync(FileRequest request) => new Response().ToTask();

        public override Task<Response> CreateAsync(FileRequest request) => new Response().ToTask();

        public override Task<Response> UpdateAsync(FileRequest request) => new Response().ToTask();

        public override Task<Response> DeleteAsync(FileRequest request) => new Response().ToTask();
    }

    public class TestHttpController : ResourceController<HttpRequest>
    {
        public override Task<Response> ReadAsync(HttpRequest request) => new Response().ToTask();

        public override Task<Response> CreateAsync(HttpRequest request) => new Response().ToTask();

        public override Task<Response> UpdateAsync(HttpRequest request) => new Response().ToTask();

        public override Task<Response> DeleteAsync(HttpRequest request) => new Response().ToTask();
    }
}