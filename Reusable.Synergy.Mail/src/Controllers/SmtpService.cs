using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Reusable.Marbles;
using Reusable.Marbles.Extensions;
using Reusable.Synergy.Requests;
using ContentDisposition = MimeKit.ContentDisposition;

namespace Reusable.Synergy.Controllers;

public abstract class SmtpService : Service
{
    public SmtpClient SmtpClient { get; set; } = new();

    public class Send : SmtpService
    {
        public override async Task<object> InvokeAsync(IRequest request)
        {
            var mail = ThrowIfNot<SendMailWithSmtp>(request);

            using var message = new MimeMessage();
            message.From.Add(new MailboxAddress(mail.From, mail.From));
            message.To.AddRange(mail.To.Where(Conditional.IsNotNullOrEmpty).Select(x => new MailboxAddress(x, x)));
            message.Cc.AddRange(mail.CC.Where(Conditional.IsNotNullOrEmpty).Select(x => new MailboxAddress(x, x)));
            message.Subject = mail.Subject;

            var multipart = new Multipart("mixed")
            {
                new TextPart(mail.IsHtml ? TextFormat.Html : TextFormat.Plain)
                {
                    Text = mail.Body switch
                    {
                        string text => text,
                        Stream stream => await stream.ReadTextAsync(mail.Encoding),
                        _ => string.Empty
                    }
                },
            };

            foreach (var (key, value) in mail.Attachments)
            {
                multipart.Add(new MimePart(MediaTypeNames.Application.Octet)
                {
                    Content = new MimeContent(new MemoryStream(value).Rewind()),
                    ContentDisposition = new ContentDisposition(ContentDisposition.Attachment),
                    ContentTransferEncoding = ContentEncoding.Base64,
                    FileName = key
                });
            }

            message.Body = multipart;

            if (!SmtpClient.IsConnected)
            {
                await SmtpClient.ConnectAsync
                (
                    mail.Host,
                    mail.Port,
                    mail.UseSsl
                );
            }

            await SmtpClient.SendAsync(message);

            return Unit.Default;
        }

        public override void Dispose()
        {
            SmtpClient.Disconnect(true);
            SmtpClient.Dispose();
        }
    }
}