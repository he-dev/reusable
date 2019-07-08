using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.IOnymous.Http;
using Reusable.IOnymous.Http.Mailr.Models;
using Reusable.Net.Http.Formatting;
using Reusable.Quickey;

namespace Reusable.IOnymous
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
                    .SetItem(From<IHttpMeta>.Select(x => x.ConfigureRequestHeaders), headers =>
                    {
                        headers
                            .UserAgent(userAgent.ProductName, userAgent.ProductVersion)
                            .AcceptHtml();
                    })
                    .SetItem(From<IHttpMeta>.Select(x => x.ResponseFormatters), new[] { new TextMediaTypeFormatter() })
                    .SetItem(From<IHttpMeta>.Select(x => x.ContentType), "application/json");

            var response = await provider.InvokeAsync(new Request.Post(uri)
            {
                Extensions = properties,
                CreateBodyStreamFunc = email.CreateSerializeStreamFunc
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