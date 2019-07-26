using System.Collections.Immutable;
using System.IO;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.IOnymous;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace Reusable.Tests.IOnymous
{
    public class RelativeProviderTest
    {
//        [Fact]
//        public void Throws_when_base_uri_not_absolute()
//        {
//            Assert.Throws<ArgumentException>(() => new InMemoryResourceProvider().DecorateWith(RelativeResourceProvider.Factory("blub")));
//        }

        [Fact]
        public async Task Creates_new_absolute_uri_when_relative_one_specified()
        {
            var mockProvider = Mock.Create<IResourceProvider>();

            mockProvider
                .Arrange(x => x.Properties)
                .Returns(ImmutableContainer.Empty);
            
            mockProvider
                .Arrange(x => x.Methods)
                .Returns(MethodCollection.Empty.Add(RequestMethod.Get, r => Resource.DoesNotExist.FromRequest(r).ToTask()));
            
//            mockProvider
//                .Arrange(x => x.Schemes)
//                .Returns(new SoftString[] { "blub" }.ToImmutableHashSet());

            mockProvider
                .Arrange(x => x.InvokeAsync(Arg.IsAny<Request>()));
                //.Returns<Task<IResource>>(r => Task.FromResult<IResource>(new InMemoryResource(ImmutableContainer.Empty.SetUri(""), Stream.Null)));


            var relativeProvider = mockProvider.DecorateWith(RelativeProvider.Factory("base:path"));
            var resource = await relativeProvider.InvokeAsync(new Request.Get("relative"));

            Assert.False(resource.Exists);
            Assert.Equal("base:path/relative", resource.Uri);
        }
    }
}