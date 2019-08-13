using System;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.IOnymous;
using Reusable.IOnymous.Http.Mailr.Models;
using Reusable.Teapot;
using Reusable.Utilities.XUnit.Fixtures;
using Xunit;

namespace Reusable.IOnymous.Http.Mailr
{
    public class HttpProviderExtensionsTest : IDisposable, IClassFixture<TeapotServerFixture>
    {
        private readonly ITeapotServerContext _serverContext;

        private readonly IResourceRepository _resources;

        public HttpProviderExtensionsTest(TeapotServerFixture teapotServerFixture)
        {
            _serverContext = teapotServerFixture.GetServer("http://localhost:30002").BeginScope();
            _resources = new ResourceRepository(b => b.UseResources(HttpProvider.FromBaseUri("http://localhost:30002/api", ImmutableContainer.Empty.AddTag("Mailr"))));
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
                        })
                        .Occurs(1);
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

            var response = await _resources.SendEmailAsync("mailr/messages/test", new UserAgent("xunit", "1.0"), email, "Mailr");

            _serverContext.Assert();
            Assert.Equal("OK!", response);
        }

        public void Dispose()
        {
            _serverContext.Dispose();
            _resources.Dispose();
        }
    }

    internal class TeapotAssert : Assert
    {
        public static void ImATeapot(Exception ex) => Equal("Response status code does not indicate success: 418 (I'm a teapot).", ex.Message);
    }
}