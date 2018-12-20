using System;
using System.Configuration;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Reusable.sdk.Http;
using Reusable.sdk.Mailr;
using Reusable.Teapot;
using Reusable.Utilities.XUnit.Fixtures;
using Xunit;

namespace Reusable.Tests.XUnit.sdk.Mailr
{
    public class EmailsTest : IClassFixture<TeapotFactoryFixture>
    {
        private const string Url = "http://localhost:12000";

        private readonly TeapotServer _server;

        public EmailsTest(TeapotFactoryFixture teapotFactory)
        {
            _server = teapotFactory.CreateTeapotServer(Url);
        }

        [Fact]
        public async Task SendAsync_CanRequestMessagesResource()
        {
            try
            {
                var mailr = RestClient.Create<IMailrClient>(Url + "/api", headers =>
                {
                    headers.AcceptJson();
                    headers.UserAgent(productName: "Reusable.Tests2", productVersion: "3.0");
                });

                var body =
                    await
                        mailr
                            .Resource("mailr", "messages", "test")
                            .SendAsync(Email.CreateHtml("myemail@mail.com", "Testmail", new { Greeting = "Hallo Mailr!" }, email => email.CanSend = false));
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
            var mailr = RestClient.Create<IMailrClient>(ConfigurationManager.AppSettings["mailr:BaseUri"], headers =>
            {
                headers.AcceptJson();
                headers.UserAgent(productName: "MailrNET.Tests", productVersion: "3.0.0");
            });

            var body =
                await
                    mailr
                        .Resource("mailr", "messages", "test")
                        .SendAsync(Email.CreateHtml("...@gmail.com", "Testmail", new { Greeting = "Hallo Mailr!" }, email => email.CanSend = false));


            var asdf = 0;
        }
    }

    internal class TeapotAssert : Assert
    {
        public static void ImATeapot(Exception ex) => Equal("Response status code does not indicate success: 418 (I'm a teapot).", ex.Message);      
    }

    public interface IJsonSection
    {
        JToken Value { get; }
    }

    public readonly struct JsonSection : IJsonSection
    {
        public JsonSection(JToken value) => Value = value;

        public JToken Value { get; }
    }

    public static class RequestInfoExtensions
    {
        public static void Body(this RequestInfo request, Action<IJsonSection> section)
        {
            section(new JsonSection(request.Parse()));
        }

        public static void HasProperty(this IJsonSection section, string jsonPath)
        {
            Assert.False(section.Value.SelectToken(jsonPath) == null, $"Property '{jsonPath}' not found.");
        }

        public static void PropertyEqual(this IJsonSection section, string jsonPath, object expected)
        {
            if (section.Value.SelectToken(jsonPath) is JValue actual)
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

        private static JToken Parse(this RequestInfo info)
        {
            if (info.ContentLength == 0)
            {
                // This supports the null-pattern.
                return JToken.Parse("{}");
            }

            using (var memory = new MemoryStream())
            {
                // It needs to be copied because otherwise it'll get disposed.
                info.BodyStreamCopy.Seek(0, SeekOrigin.Begin);
                info.BodyStreamCopy.CopyTo(memory);

                // Rewind to read from the beginning.
                memory.Seek(0, SeekOrigin.Begin);
                using (var reader = new StreamReader(memory))
                {
                    var body = reader.ReadToEnd();
                    return JToken.Parse(body);
                }
            }
        }
    }
}
