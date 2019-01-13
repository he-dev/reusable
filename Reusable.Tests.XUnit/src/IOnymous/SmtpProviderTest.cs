using System.Threading.Tasks;
using Reusable.IOnymous;
using Xunit;

namespace Reusable.Tests.XUnit.IOnymous
{
    public class SmtpProviderTest
    {
        //[Fact]
        public async Task Can_send_email()
        {
            var smtp = new SmtpProvider();
            await smtp.SendEmailAsync(new Email<IEmailSubject, IEmailBody>
            {
                To = new[] { "you@example.com" },
                From = "me@example.com",
                Subject = new EmailSubject
                {
                    Value = "Test"
                },
                Body = new EmailBody
                {
                    Value = "Hi!"
                }
            });
        }
    }
}