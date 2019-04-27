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
                metadata
                    .Scope<IMailSession>(m => m
                        .Set(x => x.From, email.From)
                        .Set(x => x.To, email.To)
                        .Set(x => x.CC, email.CC)
                        .Set(x => x.Subject, email.Subject.Value)
                        .Set(x => x.Attachments, email.Attachments)
                        .Set(x => x.From, email.From)
                        .Set(x => x.IsHtml, email.IsHtml)
                        .Set(x => x.IsHighPriority, email.IsHighPriority)
                    );

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