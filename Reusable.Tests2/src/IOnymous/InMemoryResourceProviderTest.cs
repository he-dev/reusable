using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.IOnymous;
using Xunit;

namespace Reusable.Tests2.IOnymous
{
    public class InMemoryResourceProviderTest
    {
        [Fact]
        public async Task Blub()
        {
            var inMemory = new InMemoryResourceProvider(ResourceProviderMetadata.Empty)
            {
                { "foo/bar", "baz" },
                { "scheme:foo/bar", "qux" },
            };

            var value1 = await inMemory.GetAsync("foo/bar");
            var value2 = await inMemory.GetAsync("scheme:foo/bar");

            Assert.True(value1.Exists);
            Assert.True(value2.Exists);

            Assert.Equal("baz", await value1.DeserializeAsync<string>());
            Assert.Equal("qux", await value2.DeserializeAsync<string>());
        }
    }
}
