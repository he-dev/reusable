using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Reusable.Exceptionizer;
using Reusable.IOnymous;
using Xunit;

namespace Reusable.Tests.XUnit.IOnymous
{
    public class CompositeResourceProviderTest
    {
        [Fact]
        public async Task Gets_first_matching_resource_by_default()
        {
            var composite = new CompositeResourceProvider(new IResourceProvider[]
            {
                new InMemoryResourceProvider
                {
                    { "blub:123", "blub1" }
                },
                new InMemoryResourceProvider
                {
                    { "blub:123", "blub2" },
                    { "blub:123", "blub3" }
                },
            });

            var resource = await composite.GetAnyAsync("123");

            Assert.True(resource.Exists);
            Assert.Equal("blub1", await resource.DeserializeAsync<string>());
        }

        [Fact]
        public async Task Can_get_resource_by_default_name()
        {
            var composite = new CompositeResourceProvider(new IResourceProvider[]
            {
                new InMemoryResourceProvider
                {
                    { "blub:123", "blub1" }
                },
                new InMemoryResourceProvider
                {
                    { "blub:123", "blub2" },
                    { "blub:123", "blub3" }
                },
            });

            var resource = await composite.GetAsync("blub:123", ResourceMetadata.Empty.Add(ResourceMetadataKeys.ProviderDefaultName, nameof(InMemoryResourceProvider)));

            Assert.True(resource.Exists);
            Assert.Equal("blub1", await resource.DeserializeAsync<string>());
        }

        [Fact]
        public async Task Can_get_resource_by_custom_name()
        {
            var composite = new CompositeResourceProvider(new IResourceProvider[]
            {
                new InMemoryResourceProvider
                {
                    { "blub:123", "blub1" }
                },
                new InMemoryResourceProvider(ResourceMetadata.Empty.Add(ResourceMetadataKeys.ProviderCustomName, "blub"))
                {
                    { "blub:123", "blub2" },
                    { "blub:123", "blub3" }
                },
            });

            var resource = await composite.GetAsync("blub:123", ResourceMetadata.Empty.Add(ResourceMetadataKeys.ProviderCustomName, "blub"));

            Assert.True(resource.Exists);
            Assert.Equal("blub3", await resource.DeserializeAsync<string>());
        }

        [Fact]
        public async Task Throws_when_PUT_without_known_resource_provider()
        {
            var composite = new CompositeResourceProvider(new IResourceProvider[]
            {
                new InMemoryResourceProvider
                {
                    { "blub:123", "blub1" }
                },
                new InMemoryResourceProvider(ResourceMetadata.Empty.Add(ResourceMetadataKeys.ProviderCustomName, "blub"))
                {
                    { "blub:123", "blub2" },
                    { "blub:123", "blub3" }
                },
            });

            await Assert.ThrowsAnyAsync<DynamicException>(async () => await composite.PutAsync("blub:123", Stream.Null));
        }

        [Fact]
        public async Task Can_get_resource_by_scheme()
        {
            var composite = new CompositeResourceProvider(new IResourceProvider[]
            {
                new InMemoryResourceProvider(new SoftString[] { "bluba" })
                {
                    { "bluba:123", "blub1" }
                },
                new InMemoryResourceProvider(new SoftString[] { "blub" })
                {
                    { "blub:123", "blub2" },
                    { "blub:125", "blub3" }
                },
            });

            var resource = await composite.GetAsync("blub:123");

            Assert.True(resource.Exists);
            Assert.Equal("blub2", await resource.DeserializeAsync<string>());
        }
    }
}