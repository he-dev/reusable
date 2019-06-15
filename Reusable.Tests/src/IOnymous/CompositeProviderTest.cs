using System.IO;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.IOnymous;
using Reusable.Quickey;
using Xunit;

namespace Reusable.Tests.IOnymous
{
    public class CompositeProviderTest
    {
        [Fact]
        public async Task Gets_first_matching_resource_by_default()
        {
            var composite = new CompositeProvider(new IResourceProvider[]
            {
                new InMemoryProvider(new UriStringPathToStringConverter(), new[] { ResourceProvider.DefaultScheme })
                {
                    { "x/123", "blub1" }
                },
                new InMemoryProvider(new UriStringPathToStringConverter(), new[] { ResourceProvider.DefaultScheme })
                {
                    { "x/123", "blub2" },
                    { "x/123", "blub3" }
                },
            });

            var resource = await composite.GetAnyAsync("blub:x/123");

            Assert.True(resource.Exists);
            Assert.Equal("blub1", await resource.DeserializeTextAsync());
        }

        [Fact]
        public async Task Can_get_resource_by_default_name()
        {
            var composite = new CompositeProvider(new IResourceProvider[]
            {
                new InMemoryProvider(new UriStringPathToStringConverter(), new[] { ResourceProvider.DefaultScheme })
                {
                    { "x/123", "blub1" }
                },
                new InMemoryProvider(new UriStringPathToStringConverter(), new[] { ResourceProvider.DefaultScheme })
                {
                    //{ "x.123", "blub2" },
                    { "x/123", "blub3" }
                },
            });

            var resource = await composite.GetAsync("blub:x/123", ImmutableSession.Empty.SetItem(From<IProviderMeta>.Select(x => x.ProviderName), "InMemoryProvider"));

            Assert.True(resource.Exists);
            Assert.Equal("blub1", await resource.DeserializeTextAsync());
        }

        [Fact]
        public async Task Can_get_resource_by_custom_name()
        {
            var composite = new CompositeProvider(new IResourceProvider[]
            {
                new InMemoryProvider(new UriStringPathToStringConverter(), new[] { ResourceProvider.DefaultScheme })
                {
                    { "x/123", "blub1" }
                },
                new InMemoryProvider(new UriStringPathToStringConverter(), new[] { ResourceProvider.DefaultScheme }, ImmutableSession.Empty.SetItem(From<IProviderMeta>.Select(x => x.ProviderName), "blub"))
                {
                    //{ "x.123", "blub2" },
                    { "x/123", "blub3" }
                },
            });

            var resource = await composite.GetAsync("blub:x/123", ImmutableSession.Empty.SetItem(From<IProviderMeta>.Select(x => x.ProviderName), "blub"));

            Assert.True(resource.Exists);
            Assert.Equal("blub3", await resource.DeserializeTextAsync());
        }

        [Fact]
        public async Task Throws_when_PUT_without_known_resource_provider()
        {
            var composite = new CompositeProvider(new IResourceProvider[]
            {
                new InMemoryProvider(new UriStringPathToStringConverter(), new[] { ResourceProvider.DefaultScheme })
                {
                    { "blub:123", "blub1" }
                },
                new InMemoryProvider(new UriStringPathToStringConverter(), new[] { ResourceProvider.DefaultScheme })
                {
                    { "blub:123?providerName=blub", "blub2" },
                    { "blub:123?providerName=blub", "blub3" }
                },
            });

            await Assert.ThrowsAnyAsync<DynamicException>(async () => await composite.PutAsync("blub:123", Stream.Null));
        }

        [Fact]
        public async Task Can_get_resource_by_scheme()
        {
            var composite = new CompositeProvider(new IResourceProvider[]
            {
                new InMemoryProvider(new UriStringPathToStringConverter(), new SoftString[] { "bluba" })
                {
                    { "x/123", "blub1" }
                },
                new InMemoryProvider(new UriStringPathToStringConverter(), new SoftString[] { "blub" })
                {
                    { "x/123", "blub2" },
                    { "x/125", "blub3" }
                },
            });

            var resource = await composite.GetAsync("blub:x/123");

            Assert.True(resource.Exists);
            Assert.Equal("blub2", await resource.DeserializeTextAsync());
        }
    }
}