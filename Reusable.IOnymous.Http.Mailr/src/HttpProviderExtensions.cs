using System.Threading.Tasks;
using Newtonsoft.Json;
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
            JsonSerializer jsonSerializer = null,
            ResourceMetadata metadata = default
        )
        {
            metadata =
                metadata
                    .Scope<HttpProvider>(scope =>
                        scope
                            .ConfigureRequestHeaders(headers =>
                            {
                                headers
                                    .UserAgent(productName, productVersion)
                                    .AcceptHtml();
                            })
                            .ResponseFormatters(new TextMediaTypeFormatter())
                            .ContentType("application/json")
                    )
                    .Schemes("http", "https")
                    // Use to pick the correct provider from a composite one.
                    .ProviderDefaultName(nameof(HttpProvider));

            var response = await resourceProvider.PostAsync(uri, () => ResourceHelper.SerializeAsJsonAsync(email, jsonSerializer), metadata);
            return await response.DeserializeTextAsync();
        }
    }
}