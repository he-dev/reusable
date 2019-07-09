using System.Net.Mime;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.IOnymous.Http.Formatting;
using Reusable.IOnymous.Http.Mailr.Models;

namespace Reusable.IOnymous.Http.Mailr
{
    public static class MailrProviderExtensions
    {
        /// <summary>
        /// Uses #Mailr
        /// </summary>
        public static IResourceProvider UseMailr(this IResourceProvider provider)
        {
            return provider.Use(ResourceProvider.CreateTag("Mailr"));
        }

        public static async Task<string> SendEmailAsync
        (
            this IResourceProvider provider,
            UriString uri,
            UserAgent userAgent,
            Email email,
            [CanBeNull] IImmutableContainer properties = default
        )
        {
            properties =
                properties
                    .ThisOrEmpty()
                    .SetItem(HttpRequestContext.ConfigureHeaders, headers =>
                    {
                        headers
                            .UserAgent(userAgent.ProductName, userAgent.ProductVersion)
                            .AcceptHtml();
                    })
                    .SetItem(HttpRequestContext.ContentType, "application/json")
                    .SetItem(HttpResponseContext.Formatters, new[] { new TextMediaTypeFormatter() })
                    .SetItem(HttpResponseContext.ContentType, "application/json");

            var response = await provider.InvokeAsync(new Request.Post(uri)
            {
                Context = properties,
                CreateBodyStreamCallback = email.CreateSerializeStreamCallback
            });

            using (response)
            {
                return await response.DeserializeTextAsync();
            }

//            using (var response = await provider.PostAsync(uri, () => ResourceHelper.SerializeAsJsonAsync(email, jsonSerializer), properties))
//            {
//                return await response.DeserializeTextAsync();
//            }
        }
    }

    public interface IMailrProperties
    {
        object Body { get; }

        string ProductName { get; }

        string ProductVersion { get; }
    }
}