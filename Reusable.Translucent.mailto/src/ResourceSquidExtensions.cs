using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Models;

namespace Reusable.Translucent
{
    public static class ResourceSquidExtensions
    {
        public static async Task<Response> SendEmailAsync
        (
            this IResourceSquid resourceSquid,
            IEmail<IEmailSubject, IEmailBody> email,
            IImmutableContainer context = default
        )
        {
            context =
                context
                    .ThisOrEmpty()
                    .SetItem(MailController.From, email.From)
                    .SetItem(MailController.To, email.To)
                    .SetItem(MailController.CC, email.CC)
                    .SetItem(MailController.Subject, email.Subject.Value)
                    .SetItem(MailController.Attachments, email.Attachments)
                    .SetItem(MailController.From, email.From)
                    .SetItem(MailController.IsHtml, email.IsHtml)
                    .SetItem(MailController.IsHighPriority, email.IsHighPriority);

            return
                await resourceSquid.InvokeAsync(new Request.Post($"{UriSchemes.Known.MailTo}:dummy@email.com")
                {
                    Body = email.Body.Value,
                    Metadata = context,
                    //CreateBodyStreamCallback = () => ResourceHelper.SerializeTextAsync(email.Body.Value, email.Body.Encoding)
                });
        }
    }
}