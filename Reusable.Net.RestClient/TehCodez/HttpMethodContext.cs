using System.Net.Http.Formatting;
using JetBrains.Annotations;

namespace Reusable.Net
{
    [PublicAPI]
    public class HttpMethodContext
    {
        [NotNull]
        public UriDynamicPart UriDynamicPart { get; set; } = new UriDynamicPart();

        [NotNull]
        public HttpRequestHeadersConfiguration HttpRequestHeadersConfiguration { get; set; } = new HttpRequestHeadersConfiguration();

        [CanBeNull]
        public object Body { get; set; }

        [NotNull]
        public MediaTypeFormatter RequestFormatter { get; set; } = new JsonMediaTypeFormatter();

        [NotNull]
        public MediaTypeFormatter ResponseFormatter { get; set; } = new JsonMediaTypeFormatter();

        public bool EnsureSuccessStatusCode { get; set; } = true;
    }
}