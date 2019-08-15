using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Exceptionize;
using Xunit;

namespace Reusable.IOnymous
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
                        { "t:a/b", "x" }
                    })
                    .Add(new InMemoryProvider
                    {
                        { "t:a/b", "y" },
                        { "t:a/b", "z" }
                    });

            var resource = await provider.GetAsync("t:a/b");

            Assert.True(resource.Exists);
            Assert.Equal("x", await resource.DeserializeTextAsync());
        }

        [Fact]
        public async Task Can_get_resource_by_custom_provider_name()
        {
            var composite =
                CompositeProvider
                    .Empty
                    .Add(new InMemoryProvider
                    {
                        { "t:a/b", "x" }
                    })
                    .Add(new InMemoryProvider(ImmutableContainer.Empty.SetName("c"))
                    {
                        { "t:a/b", "y" },
                    });

            var resource = await composite.GetAsync("t:a/b", ImmutableContainer.Empty.SetScheme(UriSchemes.Custom.IOnymous).SetName("c"));

            Assert.True(resource.Exists);
            Assert.Equal("y", await resource.DeserializeTextAsync());
        }

        [Fact]
        public async Task Throws_when_PUT_matches_multiple_resource_providers()
        {
            var composite =
                CompositeProvider
                    .Empty
                    .Add(new InMemoryProvider
                    {
                        { "t:a/b", "x" }
                    })
                    .Add(new InMemoryProvider
                    {
                        { "t:a/b", "y" }
                    });

            await Assert.ThrowsAnyAsync<DynamicException>(async () => await composite.InvokeAsync(new Request.Put("t:a/b")));
        }

        [Fact]
        public async Task Can_get_resource_by_scheme()
        {
            var composite = new CompositeProvider(new IResourceProvider[]
            {
                new InMemoryProvider(ImmutableContainer.Empty.SetScheme("s-x"))
                {
                    { "s-x:a/b", "x" }
                },
                new InMemoryProvider(ImmutableContainer.Empty.SetScheme("s-y"))
                {
                    { "s-y:a/b", "y" }
                },
            });

            var resource = await composite.GetAsync("s-y:a/b", ImmutableContainer.Empty);

            Assert.True(resource.Exists);
            Assert.Equal("y", await resource.DeserializeTextAsync());
        }
    }
}