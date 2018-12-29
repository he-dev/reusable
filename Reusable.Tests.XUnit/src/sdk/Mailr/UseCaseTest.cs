using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Reusable.IOnymous;
using Reusable.sdk.Mailr;
using Reusable.Teapot;
using Reusable.Utilities.XUnit.Fixtures;
using Xunit;

namespace Reusable.Tests.XUnit.sdk.Mailr
{
    public class UseCaseTest : IDisposable, IClassFixture<TeapotFactoryFixture>
    {
        private const string Url = "http://localhost:12000";

        private readonly TeapotServer _server;

        private readonly IResourceProvider _rest;

        public UseCaseTest(TeapotFactoryFixture teapotFactory)
        {
            _server = teapotFactory.CreateTeapotServer(Url);
            _rest = new RestResourceProvider("http://localhost:12000/api"); // ConfigurationManager.AppSettings["mailr:BaseUri"]
        }

        [Fact]
        public async Task SendAsync_CanRequestMessagesResource()
        {
            try
            {
                var content = Email.CreateHtml("myemail@mail.com", "Testmail", new { Greeting = "Hallo Mailr!" }, email => email.CanSend = false);
                var response = await _rest.SendAsync
                (
                    "mailr/messages/test",
                    content,
                    metadata: ResourceMetadata.Empty.ConfigureRequestHeaders(headers => headers.UserAgent("Reusable.Tests2", "3.0"))
                );
            }
            catch (Exception ex)
            {
                TeapotAssert.ImATeapot(ex);
            }

            //var request = _server["/mailr/messages/test"].Single();

            //request.Body(email =>
            //{
            //    email.HasProperty("$.Body.Greeting");
            //    //email.HasProperty("$.Body.Greetingg");
            //});

            //request.HasProperty("$.Body.Greetingg");
        }

        [Fact]
        public async Task SendAsync_ToSelf_GotEmail()
        {
            var content = Email.CreateHtml("...@gmail.com", "Testmail", new { Greeting = "Hallo Mailr!" }, email => email.CanSend = false);
            var response = await _rest.SendAsync
            (
                "mailr/messages/test",
                content,
                metadata: ResourceMetadata.Empty.ConfigureRequestHeaders(headers => headers.UserAgent("MailrNET.Tests", "3.0"))
            );
        }

        public void Dispose()
        {
            _rest.Dispose();
            _server.Dispose();
        }
    }

    internal class TeapotAssert : Assert
    {
        public static void ImATeapot(Exception ex) => Equal("Response status code does not indicate success: 418 (I'm a teapot).", ex.Message);
    }    
}