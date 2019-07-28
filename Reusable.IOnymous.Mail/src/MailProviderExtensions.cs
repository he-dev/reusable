using System.Threading.Tasks;
using Reusable.Data;

namespace Reusable.IOnymous.Mail
{
    public static class MailProviderExtensions
    {
        #region GET helpers

        #endregion

        #region PUT helpers

        #endregion

        #region POST helpers

        public static async Task<IResource> SendEmailAsync
        (
            this IResourceProvider provider,
            IEmail<IEmailSubject, IEmailBody> email,
            IImmutableContainer context = default
        )
        {
            context =
                context
                    .ThisOrEmpty()
                    .SetItem(MailRequestContext.From, email.From)
                    .SetItem(MailRequestContext.To, email.To)
                    .SetItem(MailRequestContext.CC, email.CC)
                    .SetItem(MailRequestContext.Subject, email.Subject.Value)
                    .SetItem(MailRequestContext.Attachments, email.Attachments)
                    .SetItem(MailRequestContext.From, email.From)
                    .SetItem(MailRequestContext.IsHtml, email.IsHtml)
                    .SetItem(MailRequestContext.IsHighPriority, email.IsHighPriority);

            return
                await provider.InvokeAsync(new Request.Post($"{UriSchemes.Known.MailTo}:dummy@email.com")
                {
                    Context = context,
                    Body = email.Body.Value,
                    CreateBodyStreamCallback = body => ResourceHelper.SerializeTextAsync((string)body, email.Body.Encoding)
                });
        }

        #endregion

        #region DELETE helpers

        #endregion
    }
}