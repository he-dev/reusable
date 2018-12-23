using System.Threading.Tasks;
using Reusable.IOnymous;
using Reusable.sdk.Http;
using Reusable.sdk.Http.Formatting;
using Xunit;

namespace Reusable.Tests.XUnit.IOnymous
{
    public class HttpResourceProviderTest
    {
        //[Fact]
        public async Task Blub()
        {
            var client = new RestResourceProvider("https://jsonplaceholder.typicode.com", ResourceMetadata.Empty);

            var response = await client.GetAsync
            (
                "posts/1",
                ResourceMetadata
                    .Empty
                    .ConfigureRequestHeaders(headers => headers.AcceptJson())
                //.ResponseFormatters(ne())
                //.ResponseType(typeof(string))
            );

            var content = await response.DeserializeStringAsync();
        }
    }
}