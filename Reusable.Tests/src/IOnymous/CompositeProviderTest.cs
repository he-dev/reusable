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
            var provider =
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

            var resource = await provider.GetAsync("test:branch/dev");

            Assert.True(resource.Exists);
            Assert.Equal("hot", await resource.DeserializeTextAsync());
        }

        [Fact]
        public async Task Gets_first_matching_resource_by_default_provider_name()
        {
            var provider =
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

            var resource = await provider.GetAsync("test:branch/dev", ImmutableContainer.Empty.SetName(nameof(InMemoryProvider)));

            Assert.True(resource.Exists);
            Assert.Equal("hot", await resource.DeserializeTextAsync());
        }

        [Fact]
        public async Task Can_get_resource_by_custom_provider_name()
        {
            var composite =
                CompositeProvider
                    .Empty
                    .Add(new InMemoryProvider
                    {
                        { "branch/dev", "cold" }
                    })
                    .Add(new InMemoryProvider(ImmutableContainer.Empty.SetName("use-this"))
                    {
                        { "branch/dev", "hot" },
                    });

            var resource = await composite.GetAsync("test:branch/dev", ImmutableContainer.Empty.SetScheme(UriSchemes.Custom.IOnymous).SetName("use-this"));

            Assert.True(resource.Exists);
            Assert.Equal("hot", await resource.DeserializeTextAsync());
        }

        [Fact]
        public async Task Throws_when_PUT_matches_multiple_resource_providers()
        {
            var composite =
                CompositeProvider
                    .Empty
                    .Add(new InMemoryProvider
                    {
                        { "branch/dev", "cold" }
                    })
                    .Add(new InMemoryProvider
                    {
                        { "branch/dev", "cold" }
                    });

            await Assert.ThrowsAnyAsync<DynamicException>(async () => await composite.InvokeAsync(new Request.Put("branch/dev")));
        }

        [Fact]
        public async Task Can_get_resource_by_scheme()
        {
            var composite = new CompositeProvider(new IResourceProvider[]
            {
                new InMemoryProvider(ImmutableContainer.Empty.SetScheme("other-one"))
                {
                    { "branch/dev", "cold" }
                },
                new InMemoryProvider(ImmutableContainer.Empty.SetScheme("this-one"))
                {
                    { "branch/dev", "hot" }
                },
            });

            var resource = await composite.GetAsync("this-one:branch/dev", ImmutableContainer.Empty);

            Assert.True(resource.Exists);
            Assert.Equal("hot", await resource.DeserializeTextAsync());
        }
    }
}