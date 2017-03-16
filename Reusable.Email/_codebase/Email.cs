using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Configuration;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Reusable
{
    public class Email<TSubject, TBody>
        where TSubject : EmailSubject
        where TBody : EmailBody
    {
        private string _from;

        private readonly Func<SmtpClient> _smtpClientFactory;

        public Email() : this(() => new SmtpClient()) { }

        public Email(Func<SmtpClient> smtpClientFactory)
        {
            _smtpClientFactory = smtpClientFactory ?? throw new ArgumentNullException(nameof(smtpClientFactory));
        }

        public string From
        {
            get
            {
                return
                    string.IsNullOrEmpty(_from)
                        ? GetFrom()
                        : _from;

                string GetFrom()
                {
                    var configuration = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                    var mailSettingsSectionGroup =
                        configuration.GetSectionGroup("system.net/mailSettings") as MailSettingsSectionGroup ??
                        throw new ArgumentNullException("From", $"You need to specify the sender either by setting the {nameof(From)} property or via app.config <system.net><mailSettings> section.");
                    return mailSettingsSectionGroup.Smtp.From;
                }
            }
            set => _from = value;
        }

        public bool IsHighPriority { get; set; }

        public TSubject Subject { get; set; }

        public TBody Body { get; set; }

        public void Send(string to) => Send(Regex.Split(to, "[,;]"));

        public void Send(IEnumerable<string> to)
        {
            using (var mailMessage = new MailMessage
            {
                Subject = (Subject ?? throw new InvalidOperationException("Subject not set.")).ToString(),
                SubjectEncoding = Subject.Encoding,

                Body = (Body ?? throw new InvalidOperationException("Body not set.")).ToString(),
                BodyEncoding = Body.Encoding,
                IsBodyHtml = Body.IsHtml,

                Priority = IsHighPriority ? MailPriority.High : MailPriority.Normal,
                From = new MailAddress(From),
            })
            {
                foreach (var address in to.Where(x => !string.IsNullOrWhiteSpace(x))) mailMessage.To.Add(new MailAddress(address.Trim()));

                using (var smtpClient = _smtpClientFactory()) smtpClient.Send(mailMessage);
            }
        }

        public override string ToString() => Body?.ToString() ?? string.Empty;
    }
}
