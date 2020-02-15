using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Reusable.Extensions;
using Reusable.Translucent.Annotations;
using Reusable.Translucent.Data;
using Reusable.Translucent.Extensions;
using ContentDisposition = MimeKit.ContentDisposition;

namespace Reusable.Translucent.Controllers
{
    public class SmtpController : MailToController
    {
        public SmtpController(ControllerName name) : base(name) { }

        [ResourcePost]
        public async Task<Response> SendEmailAsync(SmtpRequest smtp)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(smtp.From));
            message.To.AddRange(smtp.To.Where(Conditional.IsNotNullOrEmpty).Select(x => new MailboxAddress(x)));
            message.Cc.AddRange(smtp.CC.Where(Conditional.IsNotNullOrEmpty).Select(x => new MailboxAddress(x)));
            message.Subject = smtp.Subject;
            var multipart = new Multipart("mixed");
            //            {
            //                new TextPart(request.Properties.GetItemOrDefault(From<IMailMeta>.Select(x => x.IsHtml)) ? TextFormat.Html : TextFormat.Plain)
            //                {
            //                    Text = await ReadBodyAsync(body, request.Properties)
            //                }
            //            };

            using (var body = await smtp.CreateBodyStreamAsync())
            {
                multipart.Add(new TextPart(smtp.IsHtml ? TextFormat.Html : TextFormat.Plain)
                {
                    Text = await ReadBodyAsync(body, smtp)
                });
            }

            foreach (var attachment in smtp.Attachments.Where(i => i.Key.IsNotNullOrEmpty() && i.Value is {}))
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
                    smtp.Host,
                    smtp.Port,
                    smtp.UseSsl
                );
                await smtpClient.SendAsync(message);
            }

            return OK<SmtpResponse>();
        }
    }

    [PublicAPI]
    public abstract class MailToRequest : Request
    {
        public string From { get; set; } = default!;
        public List<string> To { get; set; } = new List<string>();
        public List<string> CC { get; set; } = new List<string>();
        public string Subject { get; set; } = default!;
        public Dictionary<string, byte[]> Attachments { get; set; } = new Dictionary<string, byte[]>();
        public bool IsHtml { get; set; }
        public bool IsHighPriority { get; set; }
    }

    [PublicAPI]
    public class SmtpRequest : MailToRequest
    {
        public string Host { get; set; } = default!;
        public int Port { get; set; }
        public bool UseSsl { get; set; }
    }

    public class SmtpResponse : Response { }
}