using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Quickey;

namespace Reusable.IOnymous
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
            IImmutableContainer properties = default
        )
        {
            properties =
                properties
                    .ThisOrEmpty()
                    .SetItem(From<IMailMeta>.Select(x => x.From), email.From)
                    .SetItem(From<IMailMeta>.Select(x => x.To), email.To)
                    .SetItem(From<IMailMeta>.Select(x => x.CC), email.CC)
                    .SetItem(From<IMailMeta>.Select(x => x.Subject), email.Subject.Value)
                    .SetItem(From<IMailMeta>.Select(x => x.Attachments), email.Attachments)
                    .SetItem(From<IMailMeta>.Select(x => x.From), email.From)
                    .SetItem(From<IMailMeta>.Select(x => x.IsHtml), email.IsHtml)
                    .SetItem(From<IMailMeta>.Select(x => x.IsHighPriority), email.IsHighPriority);

            return
                await provider.InvokeAsync(
                    new Request.Post($"{MailProvider.Schemes.mailto}:ionymous@mailprovider.com")
                        .SetCreateBodyStream(() => ResourceHelper.SerializeTextAsync(email.Body.Value, email.Body.Encoding))
                        .SetProperties(p => p.Union(properties)));
            
            
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