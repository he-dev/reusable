using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Extensions;

namespace Reusable.IOnymous
{
    public static class MailProviderExtensions
    {
        #region GET helpers

        public static Task<IResourceInfo> GetHttpAsync(this IResourceProvider resourceProvider, string path, Metadata metadata = default)
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

        public static async Task<IResourceInfo> SendEmailAsync(this IResourceProvider resourceProvider, IEmail<IEmailSubject, IEmailBody> email)
        {
            var metadata =
                Metadata
                    .Empty
                    .Mail
                    (
                        scope => scope
                            .From(email.From)
                            .To(email.To)
                            .CC(email.CC)
                            .Subject(email.Subject.Value)
                            .Attachments(email.Attachments)
                            .From(email.From)
                            .IsHtml(email.IsHtml)
                            .IsHighPriority(email.IsHighPriority)
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