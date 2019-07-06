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
            var resources =
                CompositeProvider
                    .Empty
                    .Add(new InMemoryProvider
                    {
                        { "branch/dev", "hot" }
                    })
                    .Add(new InMemoryProvider
                    {
                        { "branch/dev", "cold" },
                        { "branch/dev", "snow" }
                    });

            // todo - fix that
            //Assert.False(true);
            var resource = await resources.GetAsync("any-scheme:branch/dev", ImmutableContainer.Empty);

            Assert.True(resource.Exists);
            Assert.Equal("hot", await resource.DeserializeTextAsync());
        }

        [Fact]
        public async Task Can_get_resource_by_default_name()
        {
            var composite =
                CompositeProvider
                    .Empty
                    .Add(new InMemoryProvider
                    {
                        { "x/123", "blub1" }
                    })
                    .Add(new InMemoryProvider
                    {
                        //{ "x.123", "blub2" },
                        { "x/123", "blub3" }
                    });

            var resource = await composite.GetAsync("blub:x/123", ImmutableContainer.Empty.SetName("InMemoryProvider"));

            Assert.True(resource.Exists);
            Assert.Equal("blub1", await resource.DeserializeTextAsync());
        }

        [Fact]
        public async Task Can_get_resource_by_custom_name()
        {
            var composite =
                CompositeProvider
                    .Empty
                    .Add(new InMemoryProvider
                    {
                        { "x/123", "blub1" }
                    })
                    .Add(new InMemoryProvider(ImmutableContainer.Empty.SetScheme(UriSchemes.Custom.IOnymous).SetName("blub"))
                    {
                        //{ "x.123", "blub2" },
                        { "x/123", "blub3" }
                    });

            var resource = await composite.GetAsync("blub:x/123", ImmutableContainer.Empty.SetScheme(UriSchemes.Custom.IOnymous).SetName("blub"));

            Assert.True(resource.Exists);
            Assert.Equal("blub3", await resource.DeserializeTextAsync());
        }

        [Fact]
        public async Task Throws_when_PUT_matches_multiple_resource_providers()
        {
            var composite =
                CompositeProvider
                    .Empty
                    .Add(new InMemoryProvider
                    {
                        { "blub:123", "blub1" }
                    })
                    .Add(new InMemoryProvider
                    {
                        { "blub:123?providerName=blub", "blub2" },
                        { "blub:123?providerName=blub", "blub3" }
                    });

            await Assert.ThrowsAnyAsync<DynamicException>(async () => await composite.InvokeAsync(new Request.Put("blub:123")));
        }

        [Fact]
        public async Task Can_get_resource_by_scheme()
        {
            var composite = new CompositeProvider(new IResourceProvider[]
            {
                new InMemoryProvider(ImmutableContainer.Empty.SetScheme("bluba"))
                {
                    { "x/123", "blub1" }
                },
                new InMemoryProvider(ImmutableContainer.Empty.SetScheme("blub"))
                {
                    { "x/123", "blub2" },
                    { "x/125", "blub3" }
                },
            });

            var resource = await composite.GetAsync("blub:x/123", ImmutableContainer.Empty);

            Assert.True(resource.Exists);
            Assert.Equal("blub2", await resource.DeserializeTextAsync());
        }
    }
}