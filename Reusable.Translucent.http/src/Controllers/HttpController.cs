using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Quickey;
using Reusable.Utilities.JsonNet.Converters;
using Reusable.Utilities.JsonNet.Extensions;

namespace Reusable.Translucent.Controllers
{
    public class HttpController : ResourceController
    {
        private readonly HttpClient _client;

        public HttpController(HttpClient httpClient, IImmutableContainer metadata = default)
            : base(
                metadata
                    .ThisOrEmpty()
                    .UpdateItem(Schemes, s => s
                        .Add(UriSchemes.Known.Http)
                        .Add(UriSchemes.Known.Https))
                    .SetItem(SupportsRelativeUri, true))
        {
            _client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _client.DefaultRequestHeaders.Clear();
        }

        public string BaseUri => _client.BaseAddress.ToString();

        public JsonSerializer Serializer { get; set; } = new JsonSerializer
        {
            Converters =
            {
                //new JsonStringConverter(typeof(SoftString)),
                new SoftStringConverter(),
                new StringEnumConverter()
            }
        };

        /// <summary>
        /// Create a HttpProvider that doesn't use a proxy for requests.
        /// </summary>
        public static HttpController FromBaseUri(string baseUri, IImmutableContainer properties = default)
        {
            return new HttpController(new HttpClient(new HttpClientHandler { UseProxy = false })
            {
                BaseAddress = new Uri(baseUri)
            }, properties);
        }

        [ResourceGet]
        public async Task<Response> GetAsync(Request request) => await CreateRequestCallback(HttpMethod.Get)(request);

        [ResourcePut]
        public async Task<Response> PutAsync(Request request) => await CreateRequestCallback(HttpMethod.Put)(request);

        [ResourcePost]
        public async Task<Response> PostAsync(Request request) => await CreateRequestCallback(HttpMethod.Post)(request);

        [ResourceDelete]
        public async Task<Response> DeleteAsync(Request request) => await CreateRequestCallback(HttpMethod.Delete)(request);

        public InvokeDelegate CreateRequestCallback(HttpMethod httpMethod)
        {
            return async request =>
            {
                var uri = BaseUri + request.Uri;
                var (response, contentType) = await InvokeAsync(uri, httpMethod, request.Body, request.Metadata);
                return OK(response, request.Metadata.SetItem(HttpRequest.ContentType, contentType));
            };
        }

        private async Task<(Stream Content, string ContentType)> InvokeAsync(UriString uri, HttpMethod method, object body, IImmutableContainer context)
        {
            using (var request = new HttpRequestMessage(method, uri))
            using (var content = (body is null ? Stream.Null : Serializer.Serialize(body)))
            {
                if (content != Stream.Null)
                {
                    request.Content = new StreamContent(content.Rewind());
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue(context.GetItem(HttpRequest.ContentType));
                }

                Properties.GetItemOrDefault(HttpRequest.ConfigureHeaders, _ => { })(request.Headers);
                context.GetItemOrDefault(HttpRequest.ConfigureHeaders)(request.Headers);
                using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, context.GetItemOrDefault(Request.CancellationToken)).ConfigureAwait(false))
                {
                    var responseContentCopy = new MemoryStream();

                    if (response.Content.Headers.ContentLength > 0)
                    {
                        await response.Content.CopyToAsync(responseContentCopy);
                    }

                    var clientErrorClass = new Range<int>(400, 499);
                    var serverErrorClass = new Range<int>(500, 599);

                    var classOfStatusCode =
                        clientErrorClass.ContainsInclusive((int)response.StatusCode)
                            ? "Client"
                            : serverErrorClass.ContainsInclusive((int)response.StatusCode)
                                ? "Server"
                                : null;

                    if (classOfStatusCode is null)
                    {
                        return (responseContentCopy, response.Content.Headers.ContentType?.MediaType);
                    }

                    using (var responseReader = new StreamReader(responseContentCopy.Rewind()))
                    {
                        throw DynamicException.Create
                        (
                            classOfStatusCode,
                            $"StatusCode: {(int)response.StatusCode} ({response.StatusCode}){Environment.NewLine}{await responseReader.ReadToEndAsync()}"
                        );
                    }
                }
            }
        }

        public override void Dispose()
        {
            _client.Dispose();
        }
    }

    // [UseType, UseMember]
    // [PlainSelectorFormatter]
    public class HttpRequest : Request // SelectorBuilder<HttpRequestMetadata>
    {
        #region Properties

        private static readonly From<HttpRequest> This;

        public static Selector<Action<HttpRequestHeaders>> ConfigureHeaders { get; } = This.Select(() => ConfigureHeaders);

        public static Selector<MediaTypeFormatter> RequestFormatter { get; } = This.Select(() => RequestFormatter);

        public static Selector<string> ContentType { get; } = This.Select(() => ContentType);

        #endregion
    }

    // [UseType, UseMember]
    // [PlainSelectorFormatter]
    public class HttpResponse : Response // SelectorBuilder<HttpRequestMetadata>
    {
        #region Properties

        private static readonly From<HttpResponse> This;

        public static Selector<IEnumerable<MediaTypeFormatter>> Formatters { get; } = This.Select(() => Formatters);

        public static Selector<Type> ResponseType { get; } = This.Select(() => ResponseType);

        public static Selector<string> ContentType { get; } = This.Select(() => ContentType);

        #endregion
    }
}