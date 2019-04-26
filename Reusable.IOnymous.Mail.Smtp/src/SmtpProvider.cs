using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Reusable.Extensions;

namespace Reusable.IOnymous
{
    public class SmtpProvider : MailProvider
    {
        public SmtpProvider(Metadata metadata = default) : base(metadata) { }

        protected override async Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, Metadata metadata)
        {
            var mail = metadata.Scope<IMailMetadata>(); // .Mail();

            using (var mailMessage = new MailMessage())
            {
                mailMessage.Subject = mail.Get(x => x.Subject);
                mailMessage.SubjectEncoding = Encoding.UTF8;;

                mailMessage.Body = await ReadBodyAsync(value, metadata);
                mailMessage.BodyEncoding = mail.Get(x => x.BodyEncoding);
                mailMessage.IsBodyHtml = mail.Get(x => x.IsHtml);

                mailMessage.Priority = mail.Get(x => x.IsHighPriority) ? MailPriority.High : MailPriority.Normal;
                mailMessage.From = new MailAddress(mail.Get(x => x.From));

                foreach (var attachment in mail.Get(x => x.Attachments).Where(i => i.Key.IsNotNullOrEmpty() && i.Value.IsNotNull()))
                {
                    var stream = new MemoryStream(attachment.Value).Rewind();
                    mailMessage.Attachments.Add(new Attachment(stream, attachment.Key));
                }

                foreach (var to in mail.Get(x => x.To).Where(Conditional.IsNotNullOrEmpty))
                {
                    mailMessage.To.Add(new MailAddress(to));
                }

                foreach (var cc in mail.Get(x => x.CC).Where(Conditional.IsNotNullOrEmpty))
                {
                    mailMessage.CC.Add(new MailAddress(cc));
                }

                using (var smtpClient = new SmtpClient())
                {
                    var smtp = metadata.Scope<ISmtpMetadata>();
                    smtp.AssignValueWhenExists(x => x.Host, smtpClient);
                    
                    await smtpClient.SendMailAsync(mailMessage);
                }
            }

            return new InMemoryResourceInfo(uri, metadata);
        }
    }

    public interface ISmtpMetadata : IMetadataScope
    {
        string Host { get; }

        int Port { get; }

        int Timeout { get; }

        ServicePoint ServicePoint { get; }

        ICredentialsByHost Credentials { get; }

        bool EnableSsl { get; }

        X509CertificateCollection ClientCertificates { get; }

        string TargetName { get; }
    }
}