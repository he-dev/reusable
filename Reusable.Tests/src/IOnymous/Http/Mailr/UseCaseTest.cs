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
    public class UseCaseTest : IDisposable, IClassFixture<TeapotServerFixture>
    {
        private readonly ITeapotServerContext _serverContext;

        private readonly IResourceProvider _http;

        public UseCaseTest(TeapotServerFixture teapotServerFixture)
        {
            _serverContext = teapotServerFixture.GetServer("http://localhost:30002").BeginScope();
            _http = HttpProvider.FromBaseUri("http://localhost:30002/api");
        }

        [Fact]
        public async Task Can_post_email_and_receive_html()
        {
            _serverContext
                .MockApi(HttpMethod.Post, "/api/mailr/messages/test")
                .ArrangeRequest(builder =>
                {
                    builder
                        .AcceptsHtml()
                        .AsUserAgent("IOnymous", "1.0")
                        //.WithApiVersion("1.0")
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

            var html = await _http.SendEmailAsync("mailr/messages/test", new UserAgent("IOnymous", "1.0"), email);

            Assert.Equal("OK!", html);
            _serverContext.Assert();
        }

//        [Fact]
//        public async Task SendAsync_ToSelf_GotEmail()
//        {
////            var content = Email.CreateHtml("...@gmail.com", "Testmail", new { Greeting = "Hallo Mailr!" }, email => email.CanSend = false);
////            var response = await _http.SendAsync
////            (
////                "mailr/messages/test",
////                content,
////                metadata: ResourceMetadata.Empty.ConfigureRequestHeaders(headers => headers.UserAgent("MailrNET.Tests", "3.0"))
////            );
//        }

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