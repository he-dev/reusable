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
        public SmtpProvider(IImmutableContainer properties = default) : base(properties.ThisOrEmpty())
        {
            Methods =
                MethodDictionary
                    .Empty
                    .Add(RequestMethod.Post, PostAsync);
        }

        private async Task<IResource> PostAsync(Request request)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(request.Extensions.GetItemOrDefault(From<IMailMeta>.Select(x => x.From))));
            message.To.AddRange(request.Extensions.GetItemOrDefault(From<IMailMeta>.Select(x => x.To)).Where(Conditional.IsNotNullOrEmpty).Select(x => new MailboxAddress(x)));
            message.Cc.AddRange(request.Extensions.GetItemOrDefault(From<IMailMeta>.Select(x => x.CC), Enumerable.Empty<string>().ToList()).Where(Conditional.IsNotNullOrEmpty).Select(x => new MailboxAddress(x)));
            message.Subject = request.Extensions.GetItemOrDefault(From<IMailMeta>.Select(x => x.Subject));
            var multipart = new Multipart("mixed");
//            {
//                new TextPart(request.Properties.GetItemOrDefault(From<IMailMeta>.Select(x => x.IsHtml)) ? TextFormat.Html : TextFormat.Plain)
//                {
//                    Text = await ReadBodyAsync(body, request.Properties)
//                }
//            };

            using (var body = await request.CreateBodyStreamAsync())
            {
                multipart.Add(new TextPart(request.Extensions.GetItemOrDefault(From<IMailMeta>.Select(x => x.IsHtml)) ? TextFormat.Html : TextFormat.Plain)
                {
                    Text = await ReadBodyAsync(body, request.Extensions)
                });
            }

            foreach (var attachment in request.Extensions.GetItemOrDefault(From<IMailMeta>.Select(x => x.Attachments), new Dictionary<string, byte[]>()).Where(i => i.Key.IsNotNullOrEmpty() && i.Value.IsNotNull()))
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
                    request.Extensions.GetItemOrDefault(From<ISmtpMeta>.Select(x => x.Host)),
                    request.Extensions.GetItemOrDefault(From<ISmtpMeta>.Select(x => x.Port)),
                    request.Extensions.GetItemOrDefault(From<ISmtpMeta>.Select(x => x.UseSsl), false)
                );
                await smtpClient.SendAsync(message);
            }

            return InMemoryResource.Empty.From(request);
        }
    }

    [UseType, UseMember]
    [TrimStart("I"), TrimEnd("Meta")]
    [PlainSelectorFormatter]
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