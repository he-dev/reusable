using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reusable.IOnymous;
using Reusable.sdk.Http;
using Reusable.Teapot;
using Reusable.Utilities.XUnit.Fixtures;
using Xunit;

namespace Reusable.Tests.XUnit
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
            //var client = RestClient.Create<ITestClient>("http://localhost:12000/api", headers => { headers.AcceptJson(); });
            var client = new RestResourceProvider("http://localhost:12000/api", ResourceMetadata.Empty);

            using (var teacup = _teapot.BeginScope())
            {
                teacup
                    .Responses("/api/test?param=true", "POST", builder => { builder.Once(200, new { Message = "OK" }); });

                try
                {
                    #region Request made by the applicaation somewhere deep down the rabbit hole

                    var memoryStream = new MemoryStream();
                    var textWriter = new StreamWriter(memoryStream);
                    var jsonWriter = new JsonTextWriter(textWriter);
                    new JsonSerializer().Serialize(jsonWriter, new { Greeting = "Hallo" });
                    var response = await client.PostAsync("test?param=true", memoryStream, ResourceMetadata.Empty.ConfigureRequestHeaders(headers =>
                    {
                        headers.Add("Api-Version", "1.0");
                        headers.UserAgent("Reusable", "1.0");
                        headers.AcceptJson();
                    }));

                    Assert.True(response.Exists);

                    teacup
                        .Requested("/api/test?param=true", "POST")
                        .AsUserAgent("Reusable", "1.0")
                        .Times(1)
                        .AcceptsJson()
                        .WithApiVersion("1.0")
                        .WithContentTypeJson(content => { content.HasProperty("$.Greeting"); });

                    #endregion
                }
                catch (Exception)
                {
                    // client will throw because of the 418 status code
                }
            }
        }
    }

    public interface ITestClient
    {
    }

    public class R
    {
        public string Message { get; set; }
    }
}