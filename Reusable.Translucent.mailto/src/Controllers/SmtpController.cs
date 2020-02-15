using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using JetBrains.Annotations;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Reusable.Extensions;
using Reusable.Translucent.Data;
using Reusable.Translucent.Extensions;
using ContentDisposition = MimeKit.ContentDisposition;

namespace Reusable.Translucent.Controllers
{
    public class SmtpController : MailToController<SmtpRequest>
    {
        public SmtpController(ControllerName name) : base(name) { }

        public override async Task<Response> CreateAsync(SmtpRequest smtp)
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

            return Success<Translucent.Data.SmtpResponse>();
        }
    }
}