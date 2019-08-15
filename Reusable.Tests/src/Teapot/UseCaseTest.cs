using System.Net.Http;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Translucent;
using Reusable.Translucent.Controllers;
using Reusable.Utilities.XUnit.Fixtures;
using Xunit;

namespace Reusable.Teapot
{
    public class UseCaseTest : IClassFixture<TeapotServerFixture>
    {
        private const string BaseUri = "http://localhost:30001";

        private readonly TeapotServer _teapot;

        //private readonly IResourceProvider _http;
        private readonly IResourceSquid _resources;

        public UseCaseTest(TeapotServerFixture teapotServer)
        {
            _teapot = teapotServer.GetServer(BaseUri);
            //_http = HttpProvider.FromBaseUri($"{BaseUri}/api");
            _resources = ResourceSquid.Builder.UseController(HttpController.FromBaseUri($"{BaseUri}/api")).Build();
        }

        [Fact]
        public async Task Can_post_json()
        {
            using (var apis = _teapot.BeginScope())
            {
                apis
                    .MockApi(HttpMethod.Post, "api/test?param=true")
                    .ArrangeRequest(builder =>
                    {
                        builder
                            .AsUserAgent("Teapot", "1.0")
                            .AcceptsJson()
                            .WithApiVersion("1.0")
                            .WithContentTypeJson(content => { content.HasProperty("$.Greeting"); })
                            .Occurs(1);
                    })
                    .ArrangeResponse(builder =>
                    {
                        builder
                            .Once(200, new { Message = "OK" })
                            .Echo();
                    });

                // conflicting 'response' variables
                //{
                // Request made by the application somewhere deep down the rabbit hole
                var request = new Request.Post("test?param=true")
                {
                    Metadata = ImmutableContainer
                        .Empty
                        .SetItem(HttpRequestMetadata.ConfigureHeaders, headers =>
                        {
                            headers.ApiVersion("1.0");
                            headers.UserAgent("Teapot", "1.0");
                            headers.AcceptJson();
                        })
                        .SetItem(HttpRequestMetadata.ContentType, "application/json")
                        .SetItem(HttpResponseMetadata.ContentType, "application/json"),
                    Body = new { Greeting = "Hallo" },
                    //CreateBodyStreamCallback = b => ResourceHelper.SerializeAsJsonAsync(b)
                };

                var response = await _resources.InvokeAsync(request);

                Assert.True(response.Exists());
                //var original = await response.DeserializeJsonAsync<object>();
                //}

                //apiMocks.Assert();
                apis.Assert();
            }
        }
    }
}