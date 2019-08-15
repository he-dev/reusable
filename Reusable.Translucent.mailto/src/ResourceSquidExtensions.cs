using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Models;

namespace Reusable.Translucent
{
    // Provides CRUD APIs.
    public static partial class ResourceSquidExtensions
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
                    .SetItem(MailRequestMetadata.From, email.From)
                    .SetItem(MailRequestMetadata.To, email.To)
                    .SetItem(MailRequestMetadata.CC, email.CC)
                    .SetItem(MailRequestMetadata.Subject, email.Subject.Value)
                    .SetItem(MailRequestMetadata.Attachments, email.Attachments)
                    .SetItem(MailRequestMetadata.From, email.From)
                    .SetItem(MailRequestMetadata.IsHtml, email.IsHtml)
                    .SetItem(MailRequestMetadata.IsHighPriority, email.IsHighPriority);

            return
                await resourceSquid.InvokeAsync(new Request.Post($"{UriSchemes.Known.MailTo}:dummy@email.com")
                {
                    Metadata = context,
                    Body = email.Body.Value,
                    CreateBodyStreamCallback = body => ResourceHelper.SerializeTextAsync((string)body, email.Body.Encoding)
                });
        }
    }
}