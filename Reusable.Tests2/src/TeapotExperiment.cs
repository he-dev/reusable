using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Reusable.sdk.Http;
using Reusable.Teapot;
using Reusable.Utilities.XUnit;
using Reusable.Utilities.XUnit.Fixtures;
using Xunit;

namespace Reusable.Tests2
{
    public class TeapotExperiment : IClassFixture<TeapotFactoryFixture>
    {
        private const string Url = "http://localhost:12000";

        private readonly TeapotServer _teapot;

        public TeapotExperiment(TeapotFactoryFixture teapotFactory)
        {
            _teapot = teapotFactory.CreateTeapotServer(Url);
        }

        [Fact]
        public async Task PostsGreeting()
        {
            #region Request made by the applicaation somewhere deep down the hole

            var client = RestClient.Create<ITestClient>("http://localhost:12000/api", headers => { headers.AcceptJson(); });

            try
            {
                await client.Resource("test?param=true").Configure(context =>
                {
                    context.RequestHeadersActions.Add(headers =>
                    {
                        headers.Add("Api-Version", "1.0");
                        headers.UserAgent("Reusable", "1.0");
                    });
                    context.Body = new { Greeting = "Hallo" };
                })
                .PostAsync<object>();
            }
            catch (Exception)
            {
                // client will throw because of the 418 status code
            }

            #endregion

            _teapot
               .ClientRequested("/test?param=true")
               .AsUserAgent("Reusable", "1.0")
               .Times(1)
               .AcceptsJson()
               .WithApiVersion("1.0")
               .WithContentTypeJson(content =>
               {
                   content.HasProperty("$.Greeting");
               });

            //var request = _teapot["/test?param=true"].First();

            //request.HasApiVersion("1.0");
            //request.HasProperty("$.Greeting");
            //request.PropertyEqual("$.Greeting", "Hallo");
        }
    }

    public static class RequestInfoExtensions
    {
        public static void HasProperty(this RequestInfo request, string jsonPath)
        {
            Assert.False(request.ToJToken().SelectToken(jsonPath) == null, $"Property '{jsonPath}' not found.");
        }

        public static void PropertyEqual(this RequestInfo request, string jsonPath, object expected)
        {
            if (request.ToJToken().SelectToken(jsonPath) is JValue actual)
            {
                Assert.True(actual.Equals(actual), $"Property '{jsonPath}' value '{actual.Value}' does not equal '{expected}'.");
            }
        }

        public static void HasApiVersion(this RequestInfo request, string version)
        {
            if (request.Headers.TryGetValue("Api-Version", out var value))
            {
                Assert.True(value.Equals(version));
            }
            else
            {
                Assert.False(true, $"Invalid version. Expected: '{version}', Actual: '{value}'");
            }
        }
    }

    public interface ITestClient { }
}
