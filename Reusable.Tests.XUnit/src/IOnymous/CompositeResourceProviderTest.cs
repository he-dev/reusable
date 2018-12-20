using System.Collections.Generic;
using System.Threading.Tasks;
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

            var resource = await composite.GetAsync("blub:123");
            
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
    }
}