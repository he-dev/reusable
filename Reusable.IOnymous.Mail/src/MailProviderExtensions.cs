using System.Threading.Tasks;
using Reusable.Data;

namespace Reusable.IOnymous
{
    public static class MailProviderExtensions
    {
        #region GET helpers

        public static Task<IResourceInfo> GetHttpAsync(this IResourceProvider resourceProvider, string path, IImmutableSession metadata = default)
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

        public static async Task<IResourceInfo> SendEmailAsync
        (
            this IResourceProvider resourceProvider,
            IEmail<IEmailSubject, IEmailBody> email,
            IImmutableSession metadata = default
        )
        {
            metadata =
                (metadata ?? ImmutableSession.Empty)
                .Set(Use<IMailNamespace>.Namespace, x => x.From, email.From)
                .Set(Use<IMailNamespace>.Namespace, x => x.To, email.To)
                .Set(Use<IMailNamespace>.Namespace, x => x.CC, email.CC)
                .Set(Use<IMailNamespace>.Namespace, x => x.Subject, email.Subject.Value)
                .Set(Use<IMailNamespace>.Namespace, x => x.Attachments, email.Attachments)
                .Set(Use<IMailNamespace>.Namespace, x => x.From, email.From)
                .Set(Use<IMailNamespace>.Namespace, x => x.IsHtml, email.IsHtml)
                .Set(Use<IMailNamespace>.Namespace, x => x.IsHighPriority, email.IsHighPriority);

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