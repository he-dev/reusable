using System.Collections.Immutable;
using System.Threading.Tasks;
using Reusable.IOnymous;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace Reusable.Tests.XUnit.IOnymous
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
                .Arrange(x => x.Metadata)
                .Returns(Metadata.Empty);
            
            mockProvider
                .Arrange(x => x.Schemes)
                .Returns(new SoftString[] { "blub" }.ToImmutableHashSet());

            mockProvider
                .Arrange(x => x.GetAsync(Arg.Matches<UriString>(uri => uri == new UriString("blub:base/relative")), Arg.IsAny<Metadata>()))
                .Returns<UriString, Metadata>((uri, metadata) => Task.FromResult<IResourceInfo>(new InMemoryResourceInfo(uri, Metadata.Empty)));


            var relativeProvider = mockProvider.DecorateWith(RelativeProvider.Factory("blub:base"));
            var resource = await relativeProvider.GetAsync("relative", Metadata.Empty.AllowRelativeUri(true));

            Assert.False(resource.Exists);
            Assert.Equal("blub:base/relative", resource.Uri);
        }
    }
}