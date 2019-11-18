using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Translucent.Controllers;
using Reusable.Translucent.Models;

namespace Reusable.Translucent
{
    [PublicAPI]
    public static class ResourceRepositoryExtensions
    {
        public static async Task<Response> SendEmailAsync
        (
            this IResourceRepository resourceRepository,
            IEmail<IEmailSubject, IEmailBody> email,
            IImmutableContainer? metadata = default
        )
        {
            metadata =
                metadata
                    .ThisOrEmpty()
                    .SetItem(MailController.From, email.From)
                    .SetItem(MailController.To, email.To)
                    .SetItem(MailController.CC, email.CC)
                    .SetItem(MailController.Subject, email.Subject.Value)
                    .SetItem(MailController.Attachments, email.Attachments)
                    .SetItem(MailController.From, email.From)
                    .SetItem(MailController.IsHtml, email.IsHtml)
                    .SetItem(MailController.IsHighPriority, email.IsHighPriority)
                    .SetItem(ResourceController.Schemes, UriSchemes.Known.MailTo);

            return await resourceRepository.PostAsync($"{UriSchemes.Known.MailTo}:dummy@email.com", email.Body.Value, metadata);
        }
    }
}