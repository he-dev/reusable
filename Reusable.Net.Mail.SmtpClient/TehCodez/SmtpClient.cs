using System;
using System.Configuration;
using System.Net.Configuration;
using System.Net.Mail;
using System.Threading.Tasks;
using SystemClient = System.Net.Mail.SmtpClient;

namespace Reusable.Net.Mail
{
    /// <summary>
    /// Uses the System.Net.Mail.SmtpClient to send emails.
    /// </summary>
    public class SmtpClient : EmailClient
    {
        public string From
        {
            get
            {
                var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var mailSettingsSectionGroup = configuration.GetSectionGroup("system.net/mailSettings") as MailSettingsSectionGroup;
                return 
                    mailSettingsSectionGroup?.Smtp.From 
                    ?? throw new InvalidOperationException(
                        $"You need to specify the sender either by setting the {nameof(From)} property on the {nameof(Email<IEmailSubject, IEmailBody>)} or via app.config <system.net><mailSettings> section.");
            }
        }

        protected override async Task SendAsyncCore<TSubject, TBody>(IEmail<TSubject, TBody> email)
        {
            using (var smtpClient = new SystemClient())
            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = email.Subject.ToString();
                mailMessage.SubjectEncoding = email.Subject.Encoding;

                mailMessage.Body = email.Body.ToString();
                mailMessage.BodyEncoding = email.Body.Encoding;
                mailMessage.IsBodyHtml = email.Body.IsHtml;

                mailMessage.Priority = email.HighPriority ? MailPriority.High : MailPriority.Normal;
                mailMessage.From = new MailAddress(email?.From ?? From);

                foreach (var to in email.To.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    mailMessage.To.Add(new MailAddress(to));
                }
                
                await smtpClient.SendMailAsync(mailMessage);
            }
        }
    }

}
