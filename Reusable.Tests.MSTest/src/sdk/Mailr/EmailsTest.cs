using System.Configuration;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Reusable.sdk.Http;
using Reusable.sdk.Mailr;

namespace Reusable.Tests.Mailr
{
    [TestClass]
    public class EmailsTest
    {
        [TestMethod]
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
}
