using System.Collections.Generic;
using System.Net.Http.Headers;

namespace Reusable.Net
{
    public static class HttpRequestHeadersConfigurationExtensions
    {
        public static HttpRequestHeadersConfiguration AddRange(this HttpRequestHeadersConfiguration configuration, IDictionary<string, IEnumerable<string>> defaultHeaders)
        {
            return (configuration += headers =>
            {
                foreach (var header in defaultHeaders)
                {
                    headers.Add(header.Key, header.Value);
                }
            });
        }

        public static HttpRequestHeadersConfiguration Clear(this HttpRequestHeadersConfiguration configuration)
        {
            return (configuration += headers => headers.Clear());
        }

        public static HttpRequestHeadersConfiguration AcceptJson(this HttpRequestHeadersConfiguration configuration)
        {
            return (configuration += headers => headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")));
        }
    }
}