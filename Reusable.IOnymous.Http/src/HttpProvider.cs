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

namespace Reusable.IOnymous
{
    public class HttpProvider : ResourceProvider
    {
        public static readonly From<IHttpMeta> PropertySelector = From<IHttpMeta>.This;

        public new static readonly string DefaultScheme = "http";

        private readonly HttpClient _client;

        public HttpProvider([NotNull] string baseUri, IImmutableContainer metadata = default)
            : base(
                metadata
                    .ThisOrEmpty()
                    .SetScheme("http")
                    .SetScheme("https")
                    .SetItem(From<IProviderProperties>.Select(x => x.AllowRelativeUri), true))
        {
            if (baseUri == null) throw new ArgumentNullException(nameof(baseUri));

            _client = new HttpClient
            {
                BaseAddress = new Uri(baseUri)
            };
            _client.DefaultRequestHeaders.Clear();

            var invokeAsync = (Func<HttpMethod, RequestCallback>)(method =>
            {
                return async request =>
                {
                    var uri = BaseUri + request.Uri;
                    using (var body = await request.CreateBodyStreamAsync())
                    {
                        var (response, mediaType) = await InvokeAsync(uri, method, request.Properties.SetItem(From<IHttpMeta>.Select(x => x.Content), body));
                        return new HttpResource(request.Properties.CopyResourceProperties().SetFormat(mediaType), response);
                    }
                };
            });

            Methods =
                MethodDictionary
                    .Empty
                    .Add(RequestMethod.Get, invokeAsync(HttpMethod.Get))
                    .Add(RequestMethod.Post, invokeAsync(HttpMethod.Post))
                    .Add(RequestMethod.Put, invokeAsync(HttpMethod.Put))
                    .Add(RequestMethod.Delete, invokeAsync(HttpMethod.Delete));
        }

        public string BaseUri => _client.BaseAddress.ToString();

        private async Task<(Stream Content, MimeType MimeType)> InvokeAsync(UriString uri, HttpMethod method, IImmutableContainer metadata)
        {
            using (var request = new HttpRequestMessage(method, uri))
            {
                var content = metadata.GetItemOrDefault(From<IHttpMeta>.Select(m => m.Content));
                if (content != null)
                {
                    request.Content = new StreamContent(content.Rewind());
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue(metadata.GetItemOrDefault(From<IHttpMeta>.Select(m => m.ContentType)));
                }

                Properties.GetItemOrDefault(From<IHttpMeta>.Select(m => m.ConfigureRequestHeaders), _ => { })(request.Headers);
                metadata.GetItemOrDefault(From<IHttpMeta>.Select(m => m.ConfigureRequestHeaders))(request.Headers);
                using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, metadata.GetItemOrDefault(From<IRequestProperties>.Select(m => m.CancellationToken))))
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

    [UseType, UseMember]
    [TrimStart("I"), TrimEnd("Meta")]
    [PlainSelectorFormatter]
    public interface IHttpMeta
    {
        Stream Content { get; }

        Action<HttpRequestHeaders> ConfigureRequestHeaders { get; }

        MediaTypeFormatter RequestFormatter { get; }

        IEnumerable<MediaTypeFormatter> ResponseFormatters { get; }

        Type ResponseType { get; }

        string ContentType { get; }
    }
}