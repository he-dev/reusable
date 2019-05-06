using System.Collections.Immutable;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.Data;
using Reusable.Net.Http.Formatting;

namespace Reusable.IOnymous
{
    public static class HttpProviderExtensions
    {
        public static async Task<string> SendAsync<TBody>
        (
            this IResourceProvider resourceProvider,
            UriString uri,
            Email<TBody> email,
            string productName,
            string productVersion,
            [CanBeNull] JsonSerializer jsonSerializer = null,
            [CanBeNull] IImmutableSession metadata = default
        )
        {
            metadata =
                (metadata ?? ImmutableSession.Empty)
                .Set(Use<IHttpNamespace>.Namespace, x => x.ConfigureRequestHeaders, headers =>
                {
                    headers
                        .UserAgent(productName, productVersion)
                        .AcceptHtml();
                })
                .Set(Use<IHttpNamespace>.Namespace, x => x.ResponseFormatters, new[] { new TextMediaTypeFormatter() })
                .Set(Use<IHttpNamespace>.Namespace, x => x.ContentType, "application/json")
                .Set(Use<IAnyNamespace>.Namespace, x => x.Schemes, ImmutableHashSet<SoftString>.Empty.Add("http").Add("https"))
                // Bind this request to the http-provider.
                .Set(Use<IProviderNamespace>.Namespace, x => x.DefaultName, nameof(HttpProvider));

            using (var response = await resourceProvider.PostAsync(uri, () => ResourceHelper.SerializeAsJsonAsync(email, jsonSerializer), metadata))
            {
                return await response.DeserializeTextAsync();
            }
        }
    }
}