using System.Threading.Tasks;
using Reusable.Data;
using Reusable.IOnymous;
using Reusable.Teapot;
using Reusable.Utilities.XUnit.Fixtures;
using Xunit;

namespace Reusable.Tests.XUnit.Teapot
{
    public class UseCaseTest : IClassFixture<TeapotFactoryFixture>
    {
        private const string BaseUri = "http://localhost:501234";

        private const string ApiUri = BaseUri + "/api";

        private readonly TeapotServer _teapot;

        private readonly IResourceProvider _http;

        public UseCaseTest(TeapotFactoryFixture teapotFactory)
        {
            _teapot = teapotFactory.CreateTeapotServer(BaseUri);
            _http = new HttpProvider(ApiUri, ImmutableSession.Empty);
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

                // conflicting 'response' variables
                {
                    // Request made by the application somewhere deep down the rabbit hole
                    var response = await _http.PostAsync
                    (
                        "test?param=true",
                        () => ResourceHelper.SerializeAsJsonAsync(new { Greeting = "Hallo" }),
                        ImmutableSession.Empty.Scope<IHttpSession>(s => s.Set(x => x.ConfigureRequestHeaders, headers =>
                        {
                            headers.ApiVersion("1.0");
                            headers.UserAgent("Teapot", "1.0");
                            headers.AcceptJson();
                        }))
                    );

                    Assert.True(response.Exists);
                    var original = await response.DeserializeJsonAsync<object>();
                }
                
                test.Assert();
            }
        }
    }
}