using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using JetBrains.Annotations;

namespace Reusable.Net.Http
{
    [PublicAPI]
    public class HttpMethodContext
    {
        public HttpMethodContext(HttpMethod method, params string[] path)
            : this(method, new PartialUriBuilder(path))
        { }

        public HttpMethodContext([NotNull] HttpMethod method, [NotNull] PartialUriBuilder uriBuilder)
        {
            Method = method ?? throw new ArgumentNullException(nameof(method));
            PartialUriBuilder = uriBuilder ?? throw new ArgumentNullException(nameof(uriBuilder));
        }

        [NotNull]
        public HttpMethod Method { get; }

        [NotNull]
        public PartialUriBuilder PartialUriBuilder { get; }

        [NotNull]
        public IList<Action<HttpRequestHeaders>> RequestHeadersActions { get; } = new List<Action<HttpRequestHeaders>>();

        [CanBeNull]
        public object Body { get; set; }

        [NotNull]
        public MediaTypeFormatter RequestFormatter { get; set; } = new JsonMediaTypeFormatter();

        [NotNull]
        public IList<MediaTypeFormatter> ResponseFormatters { get; set; } = new List<MediaTypeFormatter> { new JsonMediaTypeFormatter() };

        public bool EnsureSuccessStatusCode { get; set; } = true;
    }
}