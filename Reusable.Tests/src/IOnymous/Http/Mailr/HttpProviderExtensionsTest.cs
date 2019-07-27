using System;
using System.Net.Http;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.IOnymous;
using Reusable.IOnymous.Http;
using Reusable.IOnymous.Http.Mailr;
using Reusable.IOnymous.Http.Mailr.Models;
using Reusable.Teapot;
using Reusable.Utilities.XUnit.Fixtures;
using Xunit;

namespace Reusable.Tests.IOnymous.Http.Mailr
{
    public class HttpProviderExtensionsTest : IDisposable, IClassFixture<TeapotServerFixture>
    {
        private readonly ITeapotServerContext _serverContext;

        private readonly IResourceProvider _http;

        public HttpProviderExtensionsTest(TeapotServerFixture teapotServerFixture)
        {
            _serverContext = teapotServerFixture.GetServer("http://localhost:30002").BeginScope();
            _http = HttpProvider.FromBaseUri("http://localhost:30002/api");
        }

        [Fact]
        public async Task Can_send_email_and_receive_html()
        {
            _serverContext
                .MockPost("/api/mailr/messages/test", request =>
                {
                    request
                        .AcceptsHtml()
                        .AsUserAgent("xunit", "1.0")
                        .WithContentTypeJson(content =>
                        {
                            content
                                .HasProperty("$.To")
                                .HasProperty("$.Subject")
                                //.HasProperty("$.From") // Boom! This property does not exist.
                                .HasProperty("$.Body.Greeting");
                        });
                })
                .ArrangeResponse(builder =>
                {
                    builder
                        .Once(200, "OK!");
                });

            var email = new Email.Html(new[] { "myemail@mail.com" }, "Test-mail")
            {
                Body = new { Greeting = "Hallo Mailr!" }
            };

            var response = await _http.SendEmailAsync("mailr/messages/test", new UserAgent("xunit", "1.0"), email);

            _serverContext.Assert();
            Assert.Equal("OK!", response);
        }

        public void Dispose()
        {
            _serverContext.Dispose();
            _http.Dispose();
        }
    }

    internal class TeapotAssert : Assert
    {
        public static void ImATeapot(Exception ex) => Equal("Response status code does not indicate success: 418 (I'm a teapot).", ex.Message);
    }
}