using System.Linq;
using System.Threading.Tasks;
using Reusable.Data;
using Xunit;

namespace Reusable.IOnymous
{
    public class InMemoryProviderTest
    {
        [Fact]
        public void Can_be_created_from_collection_initializer()
        {
            var inMemory = new InMemoryProvider
            {
                { "foo:bar/baz", "qux" },
            };
            
            Assert.Equal(1, (int)inMemory.Count());
        }

        [Fact]
        public void Throws_when_uri_without_scheme()
        {
//            Assert.ThrowsAny<DynamicException>(() =>
//            {
//                var inMemory = new InMemoryProvider(Configuration.DefaultUriStringConverter, new[] { ResourceProvider.DefaultScheme })
//                {
//                    { "bar.baz", "qux" },
//                };
//            });
        }

        [Fact]
        public async Task Can_get_resource_by_uri()
        {
            var inMemory = new InMemoryProvider
            {
                { "bar/baz1", "quxx" },
                { "bar/baz2", "quxy" },
            };

            var value1 = await inMemory.GetAsync("foox:bar/baz1", ImmutableContainer.Empty);
            var value2 = await inMemory.GetAsync("fooy:bar/baz2", ImmutableContainer.Empty);

            Assert.True(value1.Exists);
            Assert.True(value2.Exists);

            Assert.Equal("quxx", await value1.DeserializeTextAsync());
            Assert.Equal("quxy", await value2.DeserializeTextAsync());
        }
        
        [Fact]
        public async Task Can_get_resource_by_ionymous_uri()
        {
            var inMemory = new InMemoryProvider
            {
                { "bar/baz", "quxx" },
                //{ "bar.baz", "quxy" },
            };

            var value1 = await inMemory.GetAsync("ionymous:bar/baz", ImmutableContainer.Empty);

            Assert.True(value1.Exists);

            Assert.Equal("quxx", await value1.DeserializeTextAsync());
        }
    }
}