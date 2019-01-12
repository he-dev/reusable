using System;
using System.Configuration;
using System.Threading.Tasks;
using Reusable.IOnymous;
using Reusable.sdk.Mailr;
using Reusable.Teapot;
using Reusable.Tests.XUnit.Fixtures;
using Xunit;

namespace Reusable.Tests.XUnit.IOnymous.Http.Mailr
{
    public class UseCaseTest : IDisposable, IClassFixture<TeapotFactoryFixture>
    {
        private readonly TeapotServer _teapot;

        private readonly IResourceProvider _http;

        public UseCaseTest(TeapotFactoryFixture teapotFactory)
        {
            _teapot = teapotFactory.CreateTeapotServer(ConfigurationManager.AppSettings["teapot:BaseUri"]);
            _http = new HttpProvider(ConfigurationManager.AppSettings["mailr:BaseUri"]);
        }

        [Fact]
        public async Task Can_post_email_and_receive_html()
        {
            using (var teacup = _teapot.BeginScope())
            {
                var mailrMessagesTestMock =
                    teacup
                        .Mock("/api/mailr/messages/test")
                        .ArrangePost((request, response) =>
                        {
                            request
                                .AcceptsHtml()
                                .AsUserAgent("IOnymous", "1.0")
                                //.WithApiVersion("1.0")
                                .WithContentTypeJson(json =>
                                {
                                    json
                                        .HasProperty("$.To")
                                        .HasProperty("$.Subject")
                                        //.HasProperty("$.From") // Boom! This property does not exist.
                                        .HasProperty("$.Body.Greeting");
                                });

                            response
                                .Once(200, "OK!");
                        });

                var email = Email.CreateHtml("myemail@mail.com", "Testmail", new { Greeting = "Hallo Mailr!" });
                var html = await _http.SendAsync("mailr/messages/test", email, "IOnymous", "1.0");

                Assert.Equal("OK!", html);
                mailrMessagesTestMock.Assert();
            }
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
            _http.Dispose();
            _teapot.Dispose();
        }
    }

    internal class TeapotAssert : Assert
    {
        public static void ImATeapot(Exception ex) => Equal("Response status code does not indicate success: 418 (I'm a teapot).", ex.Message);
    }
}