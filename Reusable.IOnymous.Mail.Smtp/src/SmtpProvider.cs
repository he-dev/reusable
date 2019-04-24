using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using Reusable.Extensions;

namespace Reusable.IOnymous
{
    public class SmtpProvider : MailProvider
    {
        public SmtpProvider(Metadata metadata = default) : base(metadata) { }

        protected override async Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, Metadata metadata)
        {
            var mail = metadata.Mail();

            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = mail.Subject();
                mailMessage.SubjectEncoding = mail.SubjectEncoding();

                mailMessage.Body = await ReadBodyAsync(value, metadata);
                mailMessage.BodyEncoding = mail.BodyEncoding();
                mailMessage.IsBodyHtml = mail.IsHtml();

                mailMessage.Priority = mail.IsHighPriority() ? MailPriority.High : MailPriority.Normal;
                mailMessage.From = new MailAddress(mail.From());

                foreach (var attachment in mail.Attachments().Where(i => i.Key.IsNotNullOrEmpty() && i.Value.IsNotNull()))
                {
                    var stream = new MemoryStream(attachment.Value).Rewind();
                    mailMessage.Attachments.Add(new Attachment(stream, attachment.Key));
                }

                foreach (var to in mail.To().Where(Conditional.IsNotNullOrEmpty))
                {
                    mailMessage.To.Add(new MailAddress(to));
                }

                foreach (var cc in mail.CC().Where(Conditional.IsNotNullOrEmpty))
                {
                    mailMessage.CC.Add(new MailAddress(cc));
                }

                using (var smtpClient = new SmtpClient())
                {
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }

            return new InMemoryResourceInfo(uri, metadata);
        }
    }
}