using System;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Teapot;
using Reusable.Translucent.Controllers;
using Reusable.Utilities.Mailr;
using Reusable.Utilities.Mailr.Models;
using Reusable.Utilities.XUnit.Fixtures;
using Xunit;

namespace Reusable.Translucent.http
{
    public class HttpControllerTest : IDisposable, IClassFixture<TeapotServerFixture>
    {
        private readonly ITeapotServerContext _serverContext;

        private readonly IResourceRepository _resources;

        public HttpControllerTest(TeapotServerFixture teapotServerFixture)
        {
            _serverContext = teapotServerFixture.GetServer("http://localhost:30002").BeginScope();

            _resources = ResourceRepository.Create(c => c.AddHttp("http://localhost:30002/api", ImmutableContainer.Empty.UpdateItem(ResourceController.Tags, tags => tags.Add("Mailr"))));
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