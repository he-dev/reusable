using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Net.Mime;
using System.Threading.Tasks;
using MailKit.Net.Smtp;
using MimeKit;
using MimeKit.Text;
using Reusable.Essentials;
using Reusable.Essentials.Extensions;
using Reusable.Octopus.Data;
using Reusable.Translucent.Data;
using ContentDisposition = MimeKit.ContentDisposition;

namespace Reusable.Translucent.Controllers;

public class SmtpController : MailToController<SmtpRequest>
{
    private SmtpClient SmtpClient { get; } = new();

    protected override async Task<Response> CreateAsync(SmtpRequest request)
    {
        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(request.From));
        message.To.AddRange(request.To.EmptyIfNull().Where(Conditional.IsNotNullOrEmpty).Select(x => new MailboxAddress(x)));
        message.Cc.AddRange(request.CC.EmptyIfNull().Where(Conditional.IsNotNullOrEmpty).Select(x => new MailboxAddress(x)));
        message.Subject = request.Subject;
        var multipart = new Multipart("mixed");

        if (request is SmtpRequest.Text && request.Data.Peek() is string text)
        {
            multipart.Add(new TextPart(request.IsHtml ? TextFormat.Html : TextFormat.Plain)
            {
                Text = text
            });
        }

        if (request is SmtpRequest.Stream && request.Data.Peek() is Stream stream)
        {
            multipart.Add(new TextPart(request.IsHtml ? TextFormat.Html : TextFormat.Plain)
            {
                Text = await ReadBodyAsync(stream, request)
            });
        }


        foreach (var attachment in request.Attachments.EmptyIfNull().Where(i => i.Key.IsNotNullOrEmpty() && i.Value is { }))
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

        if (!SmtpClient.IsConnected)
        {
            await SmtpClient.ConnectAsync
            (
                request.Host,
                request.Port,
                request.UseSsl
            );
        }

        await SmtpClient.SendAsync(message);

        return Success<Translucent.Data.SmtpResponse>(request.ResourceName.Value);
    }

    public override void Dispose()
    {
        SmtpClient.Disconnect(true);
        SmtpClient.Dispose();
    }
}