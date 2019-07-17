using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Quickey;

namespace Reusable.IOnymous.Http
{
    public class HttpProvider : ResourceProvider
    {
        //public static readonly From<IHttpMeta> PropertySelector = From<IHttpMeta>.This;

        private readonly HttpClient _client;

        public HttpProvider(HttpClient httpClient, IImmutableContainer metadata = default)
            : base(
                metadata
                    .ThisOrEmpty()
                    .SetScheme(UriSchemes.Known.Http)
                    .SetScheme(UriSchemes.Known.Https)
                    .SetItem(ResourceProviderProperty.AllowRelativeUri, true))
        {
            _client = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _client.DefaultRequestHeaders.Clear();

            Methods =
                MethodDictionary
                    .Empty
                    .Add(RequestMethod.Get, CreateRequestCallback(HttpMethod.Get))
                    .Add(RequestMethod.Post, CreateRequestCallback(HttpMethod.Post))
                    .Add(RequestMethod.Put, CreateRequestCallback(HttpMethod.Put))
                    .Add(RequestMethod.Delete, CreateRequestCallback(HttpMethod.Delete));
        }

        public string BaseUri => _client.BaseAddress.ToString();

        /// <summary>
        /// Create a HttpProvider that doesn't use a proxy for requests.
        /// </summary>
        public static HttpProvider FromBaseUri(string baseUri, IImmutableContainer properties = default)
        {
            return new HttpProvider(new HttpClient(new HttpClientHandler { UseProxy = false })
            {
                BaseAddress = new Uri(baseUri)
            });
        }

        private RequestCallback CreateRequestCallback(HttpMethod httpMethod)
        {
            return async request =>
            {
                var uri = BaseUri + request.Uri;
                var (response, mediaType) = await InvokeAsync(uri, httpMethod, request.CreateBodyStreamCallback, request.Context);
                return new HttpResource(request.Context.CopyResourceProperties().SetFormat(mediaType), response);
            };
        }

        private async Task<(Stream Content, MimeType MimeType)> InvokeAsync(UriString uri, HttpMethod method, CreateStreamCallback createBodyStream, IImmutableContainer context)
        {
            using (var request = new HttpRequestMessage(method, uri))
            using (var content = await (createBodyStream?.Invoke() ?? Task.FromResult(Stream.Null)))
            {
                if (content != Stream.Null)
                {
                    request.Content = new StreamContent(content.Rewind());
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue(context.GetItemOrDefault(HttpRequestContext.ContentType));
                }

                Properties.GetItemOrDefault(HttpRequestContext.ConfigureHeaders, _ => { })(request.Headers);
                context.GetItemOrDefault(HttpRequestContext.ConfigureHeaders)(request.Headers);
                using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, context.GetItemOrDefault(AnyRequestContext.CancellationToken)))
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
    [Rename(nameof(HttpRequestContext))]
    public class HttpRequestContext : SelectorBuilder<HttpRequestContext>
    {
        public static Selector<Action<HttpRequestHeaders>> ConfigureHeaders = Select(() => ConfigureHeaders);

        public static Selector<MediaTypeFormatter> RequestFormatter = Select(() => RequestFormatter);

        public static Selector<string> ContentType = Select(() => ContentType);
    }

    [UseType, UseMember]
    [PlainSelectorFormatter]
    [Rename(nameof(HttpResponseContext))]
    public class HttpResponseContext : SelectorBuilder<HttpRequestContext>
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
                .SetExists(!(response is null)))
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