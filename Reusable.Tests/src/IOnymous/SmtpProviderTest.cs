using System.Collections.Generic;
using System.Threading.Tasks;
using Reusable.IOnymous;

namespace Reusable.Tests.XUnit.IOnymous
{
    public class SmtpProviderTest
    {
        //[Fact]
        public async Task Can_send_email()
        {
            var smtp = new SmtpProvider();

            var email = new Email<IEmailSubject, IEmailBody>
            {
                From = "from@email.com",
                To = new List<string> { "to@email.com" },
                CC = new List<string> { "cc@email.com" },
                Subject = new EmailSubject
                {
                    Value = "Test"
                },
                Body = new EmailBody
                {
                    Value = "Hi!"
                }
            };
            using (var response = await smtp.SendEmailAsync(email))
            {
                
            }
        }
    }
}