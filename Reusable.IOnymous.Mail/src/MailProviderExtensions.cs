using System.Threading.Tasks;
using Reusable.Data;

namespace Reusable.IOnymous.Mail
{
    public static class MailProviderExtensions
    {
        #region GET helpers

//        public static Task<IResource> GetHttpAsync(this IResourceProvider resourceProvider, string path, IImmutableSession properties = default)
//        {
//            var uri = new UriString(path);
//            uri =
//                uri.IsAbsolute
//                    ? uri
//                    : new UriString(MailProvider.DefaultScheme, (string)uri.Path.Original);
//
//            return resourceProvider.GetAsync(uri, properties);
//        }

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
                    CreateBodyStreamCallback = () => ResourceHelper.SerializeTextAsync(email.Body.Value, email.Body.Encoding)
                });


//            return await provider.PostAsync
//            (
//                $"{MailProvider.DefaultScheme}:ionymous@mailprovider.com",
//                () => ResourceHelper.SerializeTextAsync(email.Body.Value, email.Body.Encoding),
//                properties
//            );
        }

        #endregion

        #region DELETE helpers

        #endregion
    }
}