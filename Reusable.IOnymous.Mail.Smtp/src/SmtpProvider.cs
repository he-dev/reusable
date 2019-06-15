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
using Reusable.Quickey;
using ContentDisposition = MimeKit.ContentDisposition;

namespace Reusable.IOnymous
{
    public class SmtpProvider : MailProvider
    {
        public SmtpProvider(ImmutableSession metadata = default) : base(metadata) { }

        protected override async Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, IImmutableSession session)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(session.GetItemOrDefault(From<IMailMeta>.Select(x => x.From))));
            message.To.AddRange(session.GetItemOrDefault(From<IMailMeta>.Select(x => x.To)).Where(Conditional.IsNotNullOrEmpty).Select(x => new MailboxAddress(x)));
            message.Cc.AddRange(session.GetItemOrDefault(From<IMailMeta>.Select(x => x.CC), Enumerable.Empty<string>().ToList()).Where(Conditional.IsNotNullOrEmpty).Select(x => new MailboxAddress(x)));
            message.Subject = session.GetItemOrDefault(From<IMailMeta>.Select(x => x.Subject));
            var multipart = new Multipart("mixed")
            {
                new TextPart(session.GetItemOrDefault(From<IMailMeta>.Select(x => x.IsHtml)) ? TextFormat.Html : TextFormat.Plain)
                {
                    Text = await ReadBodyAsync(value, session)
                }
            };

            foreach (var attachment in session.GetItemOrDefault(From<IMailMeta>.Select(x => x.Attachments), new Dictionary<string, byte[]>()).Where(i => i.Key.IsNotNullOrEmpty() && i.Value.IsNotNull()))
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
                    session.GetItemOrDefault(From<ISmtpMeta>.Select(x => x.Host)),
                    session.GetItemOrDefault(From<ISmtpMeta>.Select(x => x.Port)),
                    session.GetItemOrDefault(From<ISmtpMeta>.Select(x => x.UseSsl))
                );
                await smtpClient.SendAsync(message);
            }

            return new InMemoryResourceInfo(uri, session);
        }
    }

    [UseType]
    [UseMember]
    [TrimEnd("I")]
    [TrimStart("Meta")]
    public interface ISmtpMeta : INamespace
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