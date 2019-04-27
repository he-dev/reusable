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
                    .Scope<IHttpSession>(s => s
                        .Set(x => x.ConfigureRequestHeaders, headers =>
                        {
                            headers
                                .UserAgent(productName, productVersion)
                                .AcceptHtml();
                        })
                        .Set(x => x.ResponseFormatters, new[] { new TextMediaTypeFormatter() })
                        .Set(x => x.ContentType, "application/json")
                    )
                    .Scope<IAnySession>(s => s.Set(x => x.Schemes, ImmutableHashSet<SoftString>.Empty.Add("http").Add("https")))
                    // Bind this request to the http-provider.
                    .Scope<IProviderSession>(s => s.Set(x => x.DefaultName, nameof(HttpProvider)));

            using (var response = await resourceProvider.PostAsync(uri, () => ResourceHelper.SerializeAsJsonAsync(email, jsonSerializer), metadata))
            {
                return await response.DeserializeTextAsync();
            }
        }
    }
}