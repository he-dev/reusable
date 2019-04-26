using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Reusable.Extensions;
using ContentDisposition = MimeKit.ContentDisposition;

namespace Reusable.IOnymous
{
    public class SmtpProvider : MailProvider
    {
        public SmtpProvider(Metadata metadata = default) : base(metadata) { }

//        protected override async Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, Metadata metadata)
//        {
//            var mail = metadata.Scope<IMailMetadata>();
//
//            using (var mailMessage = new MailMessage())
//            {
//                mailMessage.Subject = mail.Get(x => x.Subject);
//                mailMessage.SubjectEncoding = Encoding.UTF8;;
//
//                mailMessage.Body = await ReadBodyAsync(value, metadata);
//                mailMessage.BodyEncoding = mail.Get(x => x.BodyEncoding);
//                mailMessage.IsBodyHtml = mail.Get(x => x.IsHtml);
//
//                mailMessage.Priority = mail.Get(x => x.IsHighPriority) ? MailPriority.High : MailPriority.Normal;
//                mailMessage.From = new MailAddress(mail.Get(x => x.From));
//
//                foreach (var attachment in mail.Get(x => x.Attachments).Where(i => i.Key.IsNotNullOrEmpty() && i.Value.IsNotNull()))
//                {
//                    var stream = new MemoryStream(attachment.Value).Rewind();
//                    mailMessage.Attachments.Add(new Attachment(stream, attachment.Key));
//                }
//
//                foreach (var to in mail.Get(x => x.To).Where(Conditional.IsNotNullOrEmpty))
//                {
//                    mailMessage.To.Add(new MailAddress(to));
//                }
//
//                foreach (var cc in mail.Get(x => x.CC).Where(Conditional.IsNotNullOrEmpty))
//                {
//                    mailMessage.CC.Add(new MailAddress(cc));
//                }
//
//                using (var smtpClient = new SmtpClient())
//                {
//                    await smtpClient.SendMailAsync(mailMessage);
//                }
//            }
//
//            return new InMemoryResourceInfo(uri, metadata);
//        }

        protected override async Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, Metadata metadata)
        {
            var mail = metadata.Scope<IMailMetadata>();

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(mail.Get(x => x.From)));
            message.To.AddRange(mail.Get(x => x.To).Where(Conditional.IsNotNullOrEmpty).Select(x => new MailboxAddress(x)));
            message.Cc.AddRange(mail.Get(x => x.CC).Where(Conditional.IsNotNullOrEmpty).Select(x => new MailboxAddress(x)));
            message.Subject = mail.Get(x => x.Subject);
            var multipart = new Multipart("mixed")
            {
                new TextPart(mail.Get(x => x.IsHtml) ? TextFormat.Html : TextFormat.Plain)
                {
                    Text = await ReadBodyAsync(value, metadata)
                }
            };

            foreach (var attachment in mail.Get(x => x.Attachments).Where(i => i.Key.IsNotNullOrEmpty() && i.Value.IsNotNull()))
            {
                var attachmentPart = new MimePart(MediaTypeNames.Application.Octet)
                {
                    Content = new MimeContent(new MemoryStream(attachment.Value).Rewind()),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = attachment.Key
                };
                multipart.Add(attachmentPart);
            }

            message.Body = multipart;

            using (var smtpClient = new SmtpClient())
            {
                var smtp = metadata.Scope<ISmtpMetadata>();
                await smtpClient.ConnectAsync(smtp.Get(x => x.Host), smtp.Get(x => x.Port), smtp.Get(x => x.UseSsl));
                await smtpClient.SendAsync(message);
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

        bool UseSsl { get; }

        X509CertificateCollection ClientCertificates { get; }

        string TargetName { get; }
    }
}