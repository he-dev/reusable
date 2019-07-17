using System.Threading.Tasks;
using Reusable.Data;
using Reusable.IOnymous;
using Reusable.IOnymous.Http;
using Reusable.Quickey;

namespace Reusable.Tests.IOnymous
{
    public class HttpProviderTest
    {
        //[Fact]
        public async Task Blub()
        {
            var client = HttpProvider.FromBaseUri("https://jsonplaceholder.typicode.com");

            var response = await client.GetAsync
            (
                "posts/1",
                ImmutableContainer
                    .Empty
                    .SetItem(HttpRequestContext.ConfigureHeaders, headers => headers.AcceptJson())
                //.ResponseFormatters(ne())
                //.ResponseType(typeof(string))
            );

            var content = await response.DeserializeTextAsync();
        }
    }
}