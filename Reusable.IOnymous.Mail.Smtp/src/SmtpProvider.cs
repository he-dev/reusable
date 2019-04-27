using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Reusable.Data;
using Reusable.Extensions;
using ContentDisposition = MimeKit.ContentDisposition;

namespace Reusable.IOnymous
{
    public class SmtpProvider : MailProvider
    {
        public SmtpProvider(ImmutableSession metadata = default) : base(metadata) { }

        protected override async Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, IImmutableSession metadata)
        {
            var mail = metadata.Scope<IMailSession>();

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(mail.Get(x => x.From)));
            message.To.AddRange(mail.Get(x => x.To).Where(Conditional.IsNotNullOrEmpty).Select(x => new MailboxAddress(x)));
            message.Cc.AddRange(mail.Get(x => x.CC, Enumerable.Empty<string>().ToList()).Where(Conditional.IsNotNullOrEmpty).Select(x => new MailboxAddress(x)));
            message.Subject = mail.Get(x => x.Subject);
            var multipart = new Multipart("mixed")
            {
                new TextPart(mail.Get(x => x.IsHtml) ? TextFormat.Html : TextFormat.Plain)
                {
                    Text = await ReadBodyAsync(value, metadata)
                }
            };

            foreach (var attachment in mail.Get(x => x.Attachments, new Dictionary<string, byte[]>()).Where(i => i.Key.IsNotNullOrEmpty() && i.Value.IsNotNull()))
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
                var smtp = metadata.Scope<ISmtpSession>();
                await smtpClient.ConnectAsync(smtp.Get(x => x.Host), smtp.Get(x => x.Port), smtp.Get(x => x.UseSsl, false));
                await smtpClient.SendAsync(message);
            }

            return new InMemoryResourceInfo(uri, metadata);
        }
    }

    public interface ISmtpSession : ISessionScope
    {
        string Host { get; }

        int Port { get; }

        //int Timeout { get; }

        //ServicePoint ServicePoint { get; }

        //ICredentialsByHost Credentials { get; }

        bool UseSsl { get; }

        //X509CertificateCollection ClientCertificates { get; }

        //string TargetName { get; }
    }
}