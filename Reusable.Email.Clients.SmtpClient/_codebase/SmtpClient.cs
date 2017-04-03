using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Configuration;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using SystemClient = System.Net.Mail.SmtpClient;

namespace Reusable.Email.Clients.SmtpClient
{
    /// <summary>
    /// Uses the System.Net.Mail.SmtpClient to send emails.
    /// </summary>
    public class SmtpClient : IEmailClient
    {
        public string From
        {
            get
            {
                var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var mailSettingsSectionGroup = configuration.GetSectionGroup("system.net/mailSettings") as MailSettingsSectionGroup;
                return mailSettingsSectionGroup?.Smtp.From ?? throw new InvalidOperationException($"You need to specify the sender either by setting the {nameof(From)} property on the {nameof(Email)} or via app.config <system.net><mailSettings> section.");
            }
        }

        public void Send<TSubject, TBody>(IEmail<TSubject, TBody> email)
            where TSubject : EmailSubject
            where TBody : EmailBody
        {
            if (email == null) throw new ArgumentNullException(nameof(email));
            if (email.To == null) throw new InvalidOperationException($"You need to set {nameof(Email)}.{nameof(IEmail<TSubject, TBody>.To)} first.");
            if (email.Subject == null) throw new InvalidOperationException($"You need to set {nameof(Email)}.{nameof(IEmail<TSubject, TBody>.Subject)} first.");
            if (email.Body == null) throw new InvalidOperationException($"You need to set {nameof(Email)}.{nameof(IEmail<TSubject, TBody>.Body)} first.");

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
                smtpClient.Send(mailMessage);
            }
        }
    }

}
