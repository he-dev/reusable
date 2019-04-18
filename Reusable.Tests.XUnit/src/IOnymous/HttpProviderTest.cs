using System.Threading.Tasks;
using Reusable.IOnymous;
using Xunit;

namespace Reusable.Tests.XUnit.IOnymous
{
    public class HttpProviderTest
    {
        //[Fact]
        public async Task Blub()
        {
            var client = new HttpProvider("https://jsonplaceholder.typicode.com", Metadata.Empty);

            var response = await client.GetAsync
            (
                "posts/1",
                Metadata
                    .Empty
                    .Scope<HttpProvider>(scope => scope.ConfigureRequestHeaders(headers => headers.AcceptJson()))
                //.ResponseFormatters(ne())
                //.ResponseType(typeof(string))
            );

            var content = await response.DeserializeTextAsync();
        }
    }
}