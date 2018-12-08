using System;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Reusable.sdk.Http;
using Xunit;

namespace Reusable.Tests2
{
    public class RequestExperiment : IClassFixture<RequestLoggerFixture>
    {
        private readonly RequestLogger _server;

        public RequestExperiment(RequestLoggerFixture requestLogger)
        {
            _server = requestLogger.Server;
        }

        [Fact]
        public async Task PostsGreeting()
        {
            #region Request made by the applicaation somewhere deep down the hole

            var client = TestClient.Create("http://localhost:12000/api", headers => { headers.AcceptJson(); });

            try
            {
                await client.Resource("test?param=true").Configure(context =>
                {
                    context.RequestHeadersActions.Add(headers => headers.Add("Api-Version", "1.0"));
                    context.Body = new { Greeting = "Hallo" };
            })
                .PostAsync<object>();
            }
            catch (Exception)
            {
                // client will throw because of the 418 status code
            }

            #endregion

            var request = _server["/test?param=true"].First();

            request.HasApiVersion("1.0");
            request.HasProperty("$.Greeting");
            request.PropertyEqual("$.Greeting", "Hallo");
        }
    }


    public static class RequestLogExtensions
    {
        public static void HasProperty(this RequestInfo request, string jsonPath)
        {
            Assert.False(request.ToJson().SelectToken(jsonPath) == null, $"Property '{jsonPath}' not found.");
        }

        public static void PropertyEqual(this RequestInfo request, string jsonPath, object expected)
        {
            if (request.ToJson().SelectToken(jsonPath) is JValue actual)
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


    public class RequestLoggerFixture : IDisposable
    {
        public RequestLoggerFixture()
        {
            Server = new RequestLogger("http://localhost:12000");
        }

        public RequestLogger Server { get; }

        public void Dispose() => Server.Dispose();
    }





    public interface ITestClient : IRestClient { }

    public class TestClient : ITestClient
    {
        private readonly IRestClient _restClient;

        private TestClient(IRestClient restClient)
        {
            _restClient = restClient;
        }

        public string BaseUri => _restClient.BaseUri;

        public static ITestClient Create(string baseUri, Action<HttpRequestHeaders> configureDefaultRequestHeaders)
        {
            var restClient = new RestClient(baseUri, configureDefaultRequestHeaders);
            return new TestClient(restClient);
        }

        public Task<T> InvokeAsync<T>(HttpMethodContext context, CancellationToken cancellationToken)
        {
            return _restClient.InvokeAsync<T>(context, cancellationToken);
        }
    }
}
