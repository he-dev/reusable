using System.Threading.Tasks;
using Reusable.Data;
using Reusable.IOnymous;
using Reusable.IOnymous.Http;
using Reusable.Quickey;
using Reusable.Teapot;
using Reusable.Utilities.XUnit.Fixtures;
using Xunit;

namespace Reusable.Tests.Teapot
{
    public class UseCaseTest : IClassFixture<TeapotFactoryFixture>
    {
        private const string BaseUri = "http://localhost:2000/api";

        //private const string ApiUri = BaseUri + "/api";

        private readonly TeapotServer _teapot;

        private readonly IResourceProvider _http;

        public UseCaseTest(TeapotFactoryFixture teapotFactory)
        {
            _teapot = teapotFactory.CreateTeapotServer("http://localhost:20001/api");
            _http = HttpProvider.FromBaseUri("http://localhost:20001");
        }

        [Fact]
        public async Task Can_post_json()
        {
            using (var teacup = _teapot.BeginScope())
            {
                var test =
                    teacup
                        .Mock("/test?param=true")
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
                    var request = new Request.Post("test?param=true")
                    {
                        Context = ImmutableContainer
                            .Empty
                            .SetItem(HttpRequestContext.ConfigureHeaders, headers =>
                            {
                                headers.ApiVersion("1.0");
                                headers.UserAgent("Teapot", "1.0");
                                headers.AcceptJson();
                            })
                            .SetItem(HttpResponseContext.ContentType, "application/json"),
                        Body = new { Greeting = "Hallo" },
                        //CreateBodyStreamCallback = b => ResourceHelper.SerializeAsJsonAsync(b)
                    };

                    var response = await _http.InvokeAsync(request);

                    Assert.True(response.Exists);
                    var original = await response.DeserializeJsonAsync<object>();
                }

                test.Assert();
            }
        }
    }
}