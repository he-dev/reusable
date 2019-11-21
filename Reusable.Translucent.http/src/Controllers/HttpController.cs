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

        public HttpController(string? id, HttpClient httpClient) : base(id, UriSchemes.Known.Http, UriSchemes.Known.Https)
        {
            _client = httpClient;
            _client.DefaultRequestHeaders.Clear();
        }

        public JsonSerializer Serializer { get; set; } = new JsonSerializer
        {
            Converters =
            {
                //new JsonStringConverter(typeof(SoftString)),
                new SoftStringConverter(),
                new StringEnumConverter()
            }
        };
        
        public Action<HttpRequestHeaders>? ConfigureHeaders { get; set; }

        /// <summary>
        /// Create a HttpProvider that doesn't use a proxy for requests.
        /// </summary>
        public static HttpController FromBaseUri(string? id, string baseUri)
        {
            return new HttpController(id, new HttpClient(new HttpClientHandler { UseProxy = false })
            {
                BaseAddress = new Uri(baseUri)
            });
        }

        [ResourceGet]
        public async Task<Response> GetAsync(Request request) => await InvokeAsync(HttpMethod.Get, request);

        [ResourcePut]
        public async Task<Response> PutAsync(Request request) => await InvokeAsync(HttpMethod.Put, request);

        [ResourcePost]
        public async Task<Response> PostAsync(Request request) => await InvokeAsync(HttpMethod.Post, request);

        [ResourceDelete]
        public async Task<Response> DeleteAsync(Request request) => await InvokeAsync(HttpMethod.Delete, request);

        public async Task<Response> InvokeAsync(HttpMethod httpMethod, Request request) => await InvokeAsync(httpMethod, (HttpRequest)request);

        private async Task<Response> InvokeAsync(HttpMethod method, HttpRequest request)
        {
            var uri = BaseUri is {} baseUri && baseUri is {} ? baseUri + request.Uri : request.Uri;
            using (var message = new HttpRequestMessage(method, uri))
            using (var content = (request.Body is {} body ? Serializer.Serialize(body) : Stream.Null))
            {
                if (content != Stream.Null)
                {
                    message.Content = new StreamContent(content.Rewind());
                    message.Content.Headers.ContentType = new MediaTypeHeaderValue(request.ContentType);
                }

                ConfigureHeaders?.Invoke(message.Headers);
                request.ConfigureHeaders?.Invoke(message.Headers);
                using (var response = await _client.SendAsync(message, HttpCompletionOption.ResponseContentRead, request.CancellationToken).ConfigureAwait(false))
                {
                    var responseContentCopy = new MemoryStream();

                    if (response.Content.Headers.ContentLength > 0)
                    {
                        //response.Content.ReadAsAsync()
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
                        return new Response
                        {
                            StatusCode = ResourceStatusCode.OK,
                            //OK(responseContentCopy, response.Content.Headers.ContentType?.MediaType);
                        };
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

//    // [UseType, UseMember]
//    // [PlainSelectorFormatter]
//    public class HttpRequest : Request // SelectorBuilder<HttpRequestMetadata>
//    {
//        #region Properties
//
//        private static readonly From<HttpRequest>? This;
//
//        public static Selector<Action<HttpRequestHeaders>> ConfigureHeaders { get; } = This.Select(() => ConfigureHeaders);
//
//        public static Selector<MediaTypeFormatter> RequestFormatter { get; } = This.Select(() => RequestFormatter);
//
//        public static Selector<string> ContentType { get; } = This.Select(() => ContentType);
//
//        #endregion
//    }

    // [UseType, UseMember]
    // [PlainSelectorFormatter]
    public class HttpResponse : Response // SelectorBuilder<HttpRequestMetadata>
    {
        #region Properties

        private static readonly From<HttpResponse>? This;

        //public static Selector<IEnumerable<MediaTypeFormatter>> Formatters { get; } = This.Select(() => Formatters);

        //public static Selector<Type> ResponseType { get; } = This.Select(() => ResponseType);

        //public static Selector<string> ContentType { get; } = This.Select(() => ContentType);

        #endregion
    }
}