using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    public static class MailProviderExtensions
    {
        #region GET helpers

        public static Task<IResource> GetHttpAsync(this IResourceProvider resourceProvider, string path, IImmutableSession metadata = default)
        {
            var uri = new UriString(path);
            return resourceProvider.GetAsync
            (
                uri.IsAbsolute
                    ? uri
                    : new UriString(MailProvider.DefaultScheme, (string)uri.Path.Original),
                metadata
            );
        }

        #endregion

        #region PUT helpers

        #endregion

        #region POST helpers

        public static async Task<IResource> SendEmailAsync
        (
            this IResourceProvider resourceProvider,
            IEmail<IEmailSubject, IEmailBody> email,
            IImmutableSession metadata = default
        )
        {
            metadata =
                metadata
                    .ThisOrEmpty()
                    .SetItem(From<IMailMeta>.Select(x => x.From), email.From)
                    .SetItem(From<IMailMeta>.Select(x => x.To), email.To)
                    .SetItem(From<IMailMeta>.Select(x => x.CC), email.CC)
                    .SetItem(From<IMailMeta>.Select(x => x.Subject), email.Subject.Value)
                    .SetItem(From<IMailMeta>.Select(x => x.Attachments), email.Attachments)
                    .SetItem(From<IMailMeta>.Select(x => x.From), email.From)
                    .SetItem(From<IMailMeta>.Select(x => x.IsHtml), email.IsHtml)
                    .SetItem(From<IMailMeta>.Select(x => x.IsHighPriority), email.IsHighPriority);

            return await resourceProvider.PostAsync
            (
                $"{MailProvider.DefaultScheme}:ionymous@mailprovider.com",
                () => ResourceHelper.SerializeTextAsync(email.Body.Value, email.Body.Encoding),
                metadata
            );
        }

        #endregion

        #region DELETE helpers

        #endregion
    }
}