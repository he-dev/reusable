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

        protected override async Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, IImmutableSession session)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(session.Get(Use<IMailSession>.Namespace, x => x.From)));
            message.To.AddRange(session.Get(Use<IMailSession>.Namespace, x => x.To).Where(Conditional.IsNotNullOrEmpty).Select(x => new MailboxAddress(x)));
            message.Cc.AddRange(session.Get(Use<IMailSession>.Namespace, x => x.CC, Enumerable.Empty<string>().ToList()).Where(Conditional.IsNotNullOrEmpty).Select(x => new MailboxAddress(x)));
            message.Subject = session.Get(Use<IMailSession>.Namespace, x => x.Subject);
            var multipart = new Multipart("mixed")
            {
                new TextPart(session.Get(Use<IMailSession>.Namespace, x => x.IsHtml) ? TextFormat.Html : TextFormat.Plain)
                {
                    Text = await ReadBodyAsync(value, session)
                }
            };

            foreach (var attachment in session.Get(Use<IMailSession>.Namespace, x => x.Attachments, new Dictionary<string, byte[]>()).Where(i => i.Key.IsNotNullOrEmpty() && i.Value.IsNotNull()))
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
                await smtpClient.ConnectAsync
                (
                    session.Get(Use<ISmtpSession>.Namespace, x => x.Host),
                    session.Get(Use<ISmtpSession>.Namespace, x => x.Port),
                    session.Get(Use<ISmtpSession>.Namespace, x => x.UseSsl, false)
                );
                await smtpClient.SendAsync(message);
            }

            return new InMemoryResourceInfo(uri, session);
        }
    }

    public interface ISmtpSession : ISession
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