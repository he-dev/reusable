using System.Net.Http;
using System.Threading.Tasks;
using Reusable.Translucent;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Data;
using Reusable.Translucent.Extensions;
using Reusable.Utilities.XUnit.Fixtures;
using Xunit;

namespace Reusable.Teapot
{
    public class UseCaseTest : IClassFixture<TestHelperFixture>
    {
        private readonly TestHelperFixture _testHelper;
        private const string BaseUri = "http://localhost:30001";

        public UseCaseTest(TestHelperFixture testHelper)
        {
            _testHelper = testHelper;
        }

        [Fact]
        public async Task Can_post_json()
        {
            using var teapot = new TeapotServer(BaseUri);
            using var teacup = teapot.BeginScope();

            teacup
                .MockApi(HttpMethod.Post, "/api/test?param=true")
                .ArrangeRequest(assert =>
                {
                    assert
                        .UserAgent("Teapot", "1.0")
                        .AcceptsJson()
                        .ApiVersion("1.0")
                        .ContentTypeJsonWhere(content => { content.HasProperty("$.Greeting"); })
                        .Occurs(1);
                })
                .ArrangeResponse(factory =>
                {
                    factory
                        .Once(200, new { Message = "OK" })
                        .Echo();
                });

            // conflicting 'response' variables
            //{
            // Request made by the application somewhere deep down the rabbit hole

            var resources = new Resource(new[] { HttpController.FromBaseUri($"{BaseUri}/api") });

            using var response = await resources.CreateAsync<HttpRequest.Json>("/test?param=true", new { Greeting = "Hallo" }, http =>
            {
                http.HeaderActions.Add(headers =>
                {
                    headers.ApiVersion("1.0");
                    headers.UserAgent("Teapot", "1.0");
                });
                http.ContentType = "application/json";
            });

            Assert.True(response.Exists());
            //var original = await response.DeserializeJsonAsync<object>();
            //}

            //apiMocks.Assert();
            teacup.Assert();
        }
    }
}