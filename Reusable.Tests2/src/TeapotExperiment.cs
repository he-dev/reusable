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

            var client = RestClient.Create<ITestClient>("http://localhost:12000/api", headers => { headers.AcceptJson(); });

            var teacup = _teapot.BeginScope();
            try
            {
                #region Request made by the applicaation somewhere deep down the rabbit hole
                
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

                #endregion
            }
            catch (Exception)
            {
                // client will throw because of the 418 status code
            }
            finally
            {
                teacup
                   .ClientRequested("/test?param=true")
                   .AsUserAgent("Reusable", "1.0")
                   .Times(1)
                   .AcceptsJson()
                   .WithApiVersion("1.0")
                   .WithContentTypeJson(content =>
                   {
                       content.HasProperty("$.Greeting");
                   });

                teacup.Dispose();
            }
        }
    }

    public interface ITestClient { }
}
