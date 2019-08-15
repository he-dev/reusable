using System.Threading.Tasks;
using Reusable.Data;
using Reusable.IOnymous.Http;

namespace Reusable.IOnymous
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