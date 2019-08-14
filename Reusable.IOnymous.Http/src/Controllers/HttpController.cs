using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.Quickey;
using Reusable.Utilities.JsonNet.Converters;
using Reusable.Utilities.JsonNet.Extensions;

// ReSharper disable once CheckNamespace
namespace Reusable.IOnymous.Controllers
{
    public class HttpController : ResourceController
    {
        private readonly HttpClient _client;

        public HttpController(HttpClient httpClient, IImmutableContainer metadata = default)
            : base(
                metadata
                    .ThisOrEmpty()
                    .UpdateItem(ResourceControllerProperties.Schemes, s => s
                        .Add(UriSchemes.Known.Http)
                        .Add(UriSchemes.Known.Https))
                    .SetItem(ResourceControllerProperties.SupportsRelativeUri, true))
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
        public async Task<IResource> GetAsync(Request request) => await CreateRequestCallback(HttpMethod.Get)(request);

        [ResourcePut]
        public async Task<IResource> PutAsync(Request request) => await CreateRequestCallback(HttpMethod.Put)(request);

        [ResourcePost]
        public async Task<IResource> PostAsync(Request request) => await CreateRequestCallback(HttpMethod.Post)(request);

        [ResourceDelete]
        public async Task<IResource> DeleteAsync(Request request) => await CreateRequestCallback(HttpMethod.Delete)(request);

        public InvokeCallback CreateRequestCallback(HttpMethod httpMethod)
        {
            return async request =>
            {
                var uri = BaseUri + request.Uri;
                var (response, mediaType) = await InvokeAsync(uri, httpMethod, request.Body, request.Metadata);
                return new HttpResource(request.Metadata.Copy<ResourceProperties>().SetItem(ResourceProperties.Format, mediaType), response);
            };
        }

        private async Task<(Stream Content, MimeType MimeType)> InvokeAsync(UriString uri, HttpMethod method, object body, IImmutableContainer context)
        {
            using (var request = new HttpRequestMessage(method, uri))
            using (var content = (body is null ? Stream.Null : Serializer.Serialize(body)))
            {
                if (content != Stream.Null)
                {
                    request.Content = new StreamContent(content.Rewind());
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue(context.GetItem(HttpRequestMetadata.ContentType));
                }

                Properties.GetItemOrDefault(HttpRequestMetadata.ConfigureHeaders, _ => { })(request.Headers);
                context.GetItemOrDefault(HttpRequestMetadata.ConfigureHeaders)(request.Headers);
                using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, context.GetItemOrDefault(RequestProperty.CancellationToken)).ConfigureAwait(false))
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
                        return (responseContentCopy, MimeType.Create(Option.Unknown, response.Content.Headers.ContentType.MediaType));
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

    [UseType, UseMember]
    [PlainSelectorFormatter]
    [Rename(nameof(HttpRequestMetadata))]
    public class HttpRequestMetadata : SelectorBuilder<HttpRequestMetadata>
    {
        public static Selector<Action<HttpRequestHeaders>> ConfigureHeaders = Select(() => ConfigureHeaders);

        public static Selector<MediaTypeFormatter> RequestFormatter = Select(() => RequestFormatter);

        public static Selector<string> ContentType = Select(() => ContentType);
    }

    [UseType, UseMember]
    [PlainSelectorFormatter]
    [Rename(nameof(HttpResponseMetadata))]
    public class HttpResponseMetadata : SelectorBuilder<HttpRequestMetadata>
    {
        public static Selector<IEnumerable<MediaTypeFormatter>> Formatters = Select(() => Formatters);

        public static Selector<Type> ResponseType = Select(() => ResponseType);

        public static Selector<string> ContentType = Select(() => ContentType);
    }

    internal class HttpResource : Resource
    {
        private readonly Stream _response;

        internal HttpResource(IImmutableContainer properties, Stream response = default)
            : base(properties
                .SetItem(ResourceProperties.Exists, !(response is null)))
        {
            _response = response;
        }

        //public override long? Length => _response?.Length;

        public override async Task CopyToAsync(Stream stream)
        {
            await _response.Rewind().CopyToAsync(stream);
        }

        public override void Dispose()
        {
            _response.Dispose();
        }
    }

    //    [UseType, UseMember]
    //    [TrimStart("I"), TrimEnd("Meta")]
    //    [PlainSelectorFormatter]
    //    public interface IHttpMeta
    //    {
    //        Stream Content { get; }
    //
    //        Action<HttpRequestHeaders> ConfigureRequestHeaders { get; }
    //
    //        MediaTypeFormatter RequestFormatter { get; }
    //
    //        IEnumerable<MediaTypeFormatter> ResponseFormatters { get; }
    //
    //        Type ResponseType { get; }
    //
    //        string ContentType { get; }
    //    }
}