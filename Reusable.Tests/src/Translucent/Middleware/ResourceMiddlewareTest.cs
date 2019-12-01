using System.Threading.Tasks;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Translucent.Annotations;
using Reusable.Translucent.Controllers;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace Reusable.Translucent.Middleware
{
    public class ResourceMiddlewareTest
    {
        [Fact]
        public async Task Gets_first_matching_resource_by_default()
        {
            var c1 = Mock.Create<TestFileController>(Behavior.CallOriginal, ComplexName.Empty);
            var c2 = Mock.Create<TestFileController>(Behavior.CallOriginal, ComplexName.Empty);
            var c3 = Mock.Create<TestFileController>(Behavior.CallOriginal, ComplexName.Empty);

            Mock.Arrange(() => c1.Get(Arg.IsAny<Request>())).Returns(new Response { StatusCode = ResourceStatusCode.NotFound }.ToTask()).OccursOnce();
            Mock.Arrange(() => c2.Get(Arg.IsAny<Request>())).Returns(new Response { StatusCode = ResourceStatusCode.OK }.ToTask()).OccursOnce();
            Mock.Arrange(() => c3.Get(Arg.IsAny<Request>())).OccursNever();

            var resources = new ResourceMiddleware(c => Task.CompletedTask, new ResourceCollection(c1, c2, c3));

            await resources.InvokeAsync(new ResourceContext { Request = Request.CreateGet<FileRequest>("file:///foo") });

            c1.Assert();
            c2.Assert();
            c3.Assert();
        }

        [Fact]
        public async Task Can_filter_controllers_by_controller_id()
        {
            var c1 = Mock.Create<TestFileController>(Behavior.CallOriginal, new ComplexName("a"));
            var c2 = Mock.Create<TestFileController>(Behavior.CallOriginal, new ComplexName("b"));
            var c3 = Mock.Create<TestFileController>(Behavior.CallOriginal, new ComplexName("d"));

            Mock.Arrange(() => c1.Get(Arg.IsAny<Request>())).OccursNever();
            Mock.Arrange(() => c2.Get(Arg.IsAny<Request>())).Returns(new Response { StatusCode = ResourceStatusCode.OK }.ToTask()).OccursOnce();
            Mock.Arrange(() => c3.Get(Arg.IsAny<Request>())).OccursNever();

            var resources = new ResourceMiddleware(c => Task.CompletedTask, new ResourceCollection(c1, c2, c3));

            await resources.InvokeAsync(new ResourceContext { Request = Request.CreateGet<FileRequest>("file:///foo").Pipe(r => { r.ControllerName = new ComplexName("b"); }) });

            c1.Assert();
            c2.Assert();
            c3.Assert();
        }

        [Fact]
        public async Task Can_filter_controllers_by_request()
        {
            var c1 = Mock.Create<TestFileController>(Behavior.CallOriginal, new ComplexName("a"));
            var c2 = Mock.Create<TestHttpController>(Behavior.CallOriginal, new ComplexName("b"));
            var c3 = Mock.Create<TestFileController>(Behavior.CallOriginal, new ComplexName("d"));

            Mock.Arrange(() => c1.Get(Arg.IsAny<Request>())).OccursNever();
            Mock.Arrange(() => c2.Get(Arg.IsAny<Request>())).Returns(new Response { StatusCode = ResourceStatusCode.OK }.ToTask()).OccursOnce();
            Mock.Arrange(() => c3.Get(Arg.IsAny<Request>())).OccursNever();

            var resources = new ResourceMiddleware(c => Task.CompletedTask, new ResourceCollection(c1, c2, c3));

            await resources.InvokeAsync(new ResourceContext { Request = Request.CreateGet<HttpRequest>("///foo") });

            c1.Assert();
            c2.Assert();
            c3.Assert();
        }

        [Fact]
        public async Task Can_filter_controllers_by_tag()
        {
            var c1 = Mock.Create<TestFileController>(Behavior.CallOriginal, new ComplexName { "a" });
            var c2 = Mock.Create<TestFileController>(Behavior.CallOriginal, new ComplexName { "b" });

            Mock.Arrange(() => c1.Get(Arg.IsAny<Request>())).CallOriginal().OccursNever();
            Mock.Arrange(() => c2.Get(Arg.IsAny<Request>())).CallOriginal().OccursOnce();

            var middleware = new ResourceMiddleware(c => Task.CompletedTask, new ResourceCollection(c1, c2));

            await middleware.InvokeAsync(new ResourceContext
            {
                Request = Request.CreateGet<FileRequest>("file:///foo").Pipe(r => { r.ControllerName = new ComplexName { "b" }; })
            });

            c1.Assert();
            c2.Assert();
        }

        [Fact]
        public async Task Throws_when_PUT_matches_multiple_controllers()
        {
            var c1 = Mock.Create<TestFileController>(Behavior.CallOriginal, ComplexName.Empty);
            var c2 = Mock.Create<TestFileController>(Behavior.CallOriginal, ComplexName.Empty);
            var c3 = Mock.Create<TestFileController>(Behavior.CallOriginal, ComplexName.Empty);

            Mock.Arrange(() => c1.Get(Arg.IsAny<Request>())).OccursNever();
            Mock.Arrange(() => c2.Get(Arg.IsAny<Request>())).OccursNever();
            Mock.Arrange(() => c3.Get(Arg.IsAny<Request>())).OccursNever();

            var resources = new ResourceMiddleware(c => Task.CompletedTask, new ResourceCollection(c1, c2, c3));

            await Assert.ThrowsAnyAsync<DynamicException>(async () => await resources.InvokeAsync(new ResourceContext { Request = Request.CreatePut<FileRequest>("file:///foo") }));

            c1.Assert();
            c2.Assert();
            c3.Assert();
        }
    }

    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    [Handles(typeof(FileRequest))]
    public class TestFileController : ResourceController
    {
        public TestFileController(ComplexName name) : base(name, "file") { }

        [ResourceGet]
        public virtual Task<Response> Get(Request request) => new Response().ToTask();

        [ResourcePost]
        public virtual Task<Response> Post(Request request) => new Response().ToTask();

        [ResourcePut]
        public virtual Task<Response> Put(Request request) => new Response().ToTask();

        [ResourceDelete]
        public virtual Task<Response> Delete(Request request) => new Response().ToTask();
    }

    [Handles(typeof(HttpRequest))]
    public class TestHttpController : ResourceController
    {
        public TestHttpController(ComplexName name) : base(name, "http") { }

        [ResourceGet]
        public virtual Task<Response> Get(Request request) => new Response().ToTask();

        [ResourcePost]
        public virtual Task<Response> Post(Request request) => new Response().ToTask();

        [ResourcePut]
        public virtual Task<Response> Put(Request request) => new Response().ToTask();

        [ResourceDelete]
        public virtual Task<Response> Delete(Request request) => new Response().ToTask();
    }
}