using System.Threading.Tasks;
using Reusable.Data;
using Reusable.IOnymous;
using Reusable.Quickey;

namespace Reusable.Tests.IOnymous
{
    public class HttpProviderTest
    {
        //[Fact]
        public async Task Blub()
        {
            var client = new HttpProvider("https://jsonplaceholder.typicode.com", ImmutableSession.Empty);

            var response = await client.GetAsync
            (
                "posts/1",
                ImmutableSession
                    .Empty
                    .SetItem(From<IHttpMeta>.Select(x => x.ConfigureRequestHeaders), headers => headers.AcceptJson())
                //.ResponseFormatters(ne())
                //.ResponseType(typeof(string))
            );

            var content = await response.DeserializeTextAsync();
        }
    }
}