using System.Threading.Tasks;
using Reusable.Data;
using Reusable.IOnymous;

namespace Reusable.Tests.XUnit.IOnymous
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
                    .Scope<IHttpSession>(s => s.Set(x => x.ConfigureRequestHeaders, headers => headers.AcceptJson()))
                //.ResponseFormatters(ne())
                //.ResponseType(typeof(string))
            );

            var content = await response.DeserializeTextAsync();
        }
    }
}