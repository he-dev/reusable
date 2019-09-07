using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.Quickey;
using Reusable.Translucent.Controllers;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace Reusable.Translucent.Middleware
{
    public class ControllerMiddlewareTest
    {
        public void Test()
        {
            var resources = default(IResourceRepository);

            //var text = resources.ReadTextFile("path", ImmutableContainer.Empty.UpdateItem());

            var text1 = resources.WhereTags("app").ReadTextFile("path");
            var text2 = resources.ReadTextFile("path", WhereTags.Are("app"));
            //var text2 = resources.ReadTextFile("path", ResourceTags.Are("app"));
        }

        [Fact]
        public void Can_filter_controllers_by_tag()
        {
            var c1 = Mock.Create<TestController>();
            var c2 = Mock.Create<TestController>();

            Mock.Arrange(() => c1.Properties).Returns(ImmutableContainer.Empty.AddScheme("s").AddTag("a"));
            Mock.Arrange(() => c1.Get(Arg.IsAny<Request>())).OccursNever();

            Mock.Arrange(() => c2.Properties).Returns(ImmutableContainer.Empty.AddScheme("s").AddTag("b"));
            Mock.Arrange(() => c2.Get(Arg.IsAny<Request>())).OccursOnce();

            var middleware = new ResourceMiddleware(c => Task.CompletedTask, new[] { c1, c2 });

            middleware.InvokeAsync(new ResourceContext
            {
                Request = new Request.Get("s:///bar")
                {
                    Metadata = ImmutableContainer.Empty.AddTag("b")
                },
            });

            c1.Assert();
            c2.Assert();
        }
    }

    public static class WhereTags
    {
        public static IImmutableContainer Are(params SoftString[] tags)
        {
            return ImmutableContainer.Empty.SetItem(ResourceController.Tags, tags.ToImmutableHashSet());
        }
    }

    // ReSharper disable once ClassWithVirtualMembersNeverInherited.Global
    public class TestController : ResourceController
    {
        public TestController([NotNull] IImmutableContainer properties) : base(properties) { }

        [ResourceGet]
        public virtual Task<Response> Get(Request request) => default;

        [ResourcePost]
        public virtual Task<Response> Post(Request request) => default;

        [ResourcePut]
        public virtual Task<Response> Put(Request request) => default;

        [ResourceDelete]
        public virtual Task<Response> Delete(Request request) => default;
    }

    public static class ResourceRepositoryExtensions
    {
        public static IResourceRepository WhereTags(this IResourceRepository resourceRepository, params string[] tags)
        {
            return new UpdateRequestMetadata(resourceRepository, container => { return tags.Aggregate(container, (result, tag) => result.UpdateItem(ResourceController.Tags, x => x.Add(tag.ToSoftString()))); });
        }
    }

    public delegate IImmutableContainer UpdateContainerDelegate(IImmutableContainer container);

    internal class UpdateRequestMetadata : IResourceRepository
    {
        private readonly IResourceRepository _resourceRepository;
        private readonly UpdateContainerDelegate _updateContainer;

        public UpdateRequestMetadata(IResourceRepository resourceRepository, UpdateContainerDelegate updateContainerDelegate)
        {
            _resourceRepository = resourceRepository;
            _updateContainer = updateContainerDelegate;
        }

        public Task<Response> InvokeAsync(Request request)
        {
            request.Metadata = _updateContainer(request.Metadata);

            return _resourceRepository.InvokeAsync(request);
        }

        public void Dispose() => _resourceRepository.Dispose();
    }
}