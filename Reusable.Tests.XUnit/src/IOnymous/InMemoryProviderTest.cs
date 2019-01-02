using System.Linq;
using System.Threading.Tasks;
using Reusable.Exceptionizer;
using Reusable.IOnymous;
using Xunit;

namespace Reusable.Tests.XUnit.IOnymous
{
    public class InMemoryProviderTest
    {
        [Fact]
        public void Can_be_created_from_collection_initializer()
        {
            var inMemory = new InMemoryProvider(ResourceMetadata.Empty)
            {
                { "foo:bar/baz", "qux" },
            };
            
            Assert.Equal(1, (int)inMemory.Count());
        }

        [Fact]
        public void Throws_when_uri_without_scheme()
        {
            Assert.ThrowsAny<DynamicException>(() =>
            {
                var inMemory = new InMemoryProvider(ResourceMetadata.Empty)
                {
                    { "bar/baz", "qux" },
                };
            });
        }

        [Fact]
        public async Task Can_get_resource_by_uri()
        {
            var inMemory = new InMemoryProvider(ResourceMetadata.Empty)
            {
                { "foox:bar/baz", "quxx" },
                { "fooy:bar/baz", "quxy" },
            };

            var value1 = await inMemory.GetAsync("foox:bar/baz");
            var value2 = await inMemory.GetAsync("fooy:bar/baz");

            Assert.True(value1.Exists);
            Assert.True(value2.Exists);

            Assert.Equal("quxx", await value1.DeserializeTextAsync());
            Assert.Equal("quxy", await value2.DeserializeTextAsync());
        }
        
        [Fact]
        public async Task Can_get_resource_by_ionymous_uri()
        {
            var inMemory = new InMemoryProvider(ResourceMetadata.Empty)
            {
                { "foox:bar/baz", "quxx" },
                { "fooy:bar/baz", "quxy" },
            };

            var value1 = await inMemory.GetAsync("ionymous:bar/baz");

            Assert.True(value1.Exists);

            Assert.Equal("quxx", await value1.DeserializeTextAsync());
        }
    }
}