using System.Threading.Tasks;
using Reusable.IOnymous;
using Reusable.Teapot;
using Reusable.Tests.XUnit.Fixtures;
using Xunit;

namespace Reusable.Tests.XUnit.Teapot
{
    public class UseCaseTest : IClassFixture<TeapotFactoryFixture>
    {
        private const string Url = "http://localhost:12000";

        private readonly TeapotServer _teapot;

        public UseCaseTest(TeapotFactoryFixture teapotFactory)
        {
            _teapot = teapotFactory.CreateTeapotServer(Url);
        }

        [Fact]
        public async Task Can_post_json()
        {
            using (var teacup = _teapot.BeginScope())
            {
                var test =
                    teacup
                        .Mock("/api/test?param=true")
                        .ArrangePost((request, response) =>
                        {
                            request
                                .AsUserAgent("Teapot", "1.0")
                                .Occurs(1)
                                .AcceptsJson()
                                .WithApiVersion("1.0")
                                .WithContentTypeJson(content => { content.HasProperty("$.Greeting"); });
                            
                            response
                                .Once(200, new { Message = "OK" })
                                .Echo();
                        });

                using (var client = new HttpProvider("http://localhost:12000/api", ResourceMetadata.Empty))
                {
                    // Request made by the application somewhere deep down the rabbit hole
                    var response = await client.PostAsync("test?param=true", () => ResourceHelper.SerializeAsJsonAsync(new { Greeting = "Hallo" }), ResourceMetadata.Empty.ConfigureRequestHeaders(headers =>
                    {
                        headers.ApiVersion("1.0");
                        headers.UserAgent("Teapot", "1.0");
                        headers.AcceptJson();
                    }));

                    Assert.True(response.Exists);
                    var original = await response.DeserializeJsonAsync<object>();
                }
                
                test.Assert();
            }
        }
    }
}