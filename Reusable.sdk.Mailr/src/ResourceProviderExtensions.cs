using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Reusable.IOnymous;
using Reusable.Net.Http.Formatting;

namespace Reusable.sdk.Mailr
{
    public static class ResourceProviderExtensions
    {
        public static async Task<string> SendAsync<TBody>
        (
            this IResourceProvider resourceProvider,
            UriString uri,
            Email<TBody> email,
            JsonSerializer jsonSerializer = null,
            ResourceMetadata metadata = null
        )
        {
            metadata =
                metadata
                .ConfigureRequestHeaders(headers => { headers.AcceptHtml(); })
                .ResponseFormatters(new TextMediaTypeFormatter())
                .Schemes("http", "https");

            var response = await resourceProvider.PostAsync(uri, () => ResourceHelper.SerializeAsJsonAsync(email, jsonSerializer), metadata);
            return await response.DeserializeTextAsync();
        }
    }
}