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

namespace Reusable.IOnymous
{
    public class HttpProvider : ResourceProvider
    {
        public new static readonly string DefaultScheme = "http";

        private readonly HttpClient _client;

        public HttpProvider([NotNull] string baseUri, ImmutableSession metadata = default)
            : base(new SoftString[] { "http", "https" }, metadata.Scope<IProviderSession>(s => s.Set(x => x.AllowRelativeUri, true)))
        {
            if (baseUri == null) throw new ArgumentNullException(nameof(baseUri));

            _client = new HttpClient
            {
                BaseAddress = new Uri(baseUri)
            };
            _client.DefaultRequestHeaders.Clear();
        }

        public string BaseUri => _client.BaseAddress.ToString();

        protected override async Task<IResourceInfo> GetAsyncInternal(UriString uri, IImmutableSession metadata)
        {
            uri = BaseUri + uri;
            var (response, mediaType) = await InvokeAsync(uri, HttpMethod.Get, metadata);
            return new HttpResourceInfo(uri, response, new MimeType(mediaType));
        }

        protected override async Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, IImmutableSession metadata)
        {
            uri = BaseUri + uri;
            var (response, mediaType) = await InvokeAsync(uri, HttpMethod.Post, metadata.Scope<IHttpSession>(s => s.Set(x => x.Content, value)));
            return new HttpResourceInfo(uri, response, new MimeType(mediaType));
        }

        #region Helpers

        private async Task<(Stream Content, string MimeType)> InvokeAsync(UriString uri, HttpMethod method, IImmutableSession metadata)
        {
            var httpMetadata = metadata.Scope<IHttpSession>();
            
            using (var request = new HttpRequestMessage(method, uri))
            {
                var content = httpMetadata.Get(x => x.Content);
                if (content != null)
                {
                    request.Content = new StreamContent(content.Rewind());
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue(httpMetadata.Get(x => x.ContentType));
                }

                Metadata.Scope<IHttpSession>().Get(x => x.ConfigureRequestHeaders, _ => {})(request.Headers);
                metadata.Scope<IHttpSession>().Get(x => x.ConfigureRequestHeaders)(request.Headers);
                using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, metadata.Scope<IAnySession>().Get(x => x.CancellationToken)))
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
                        return (responseContentCopy, response.Content.Headers.ContentType.MediaType);
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

        #endregion

        public override void Dispose()
        {
            _client.Dispose();
        }
    }

    internal class HttpResourceInfo : ResourceInfo
    {
        private readonly Stream _response;

        public HttpResourceInfo([NotNull] UriString uri, Stream response, MimeType format)
            : base(uri, ImmutableSession.Empty, s => s.Set(x => x.Format, format))
        {
            _response = response;
        }

        public override bool Exists => !(_response is null);

        public override long? Length => _response?.Length;

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn { get; }

        protected override async Task CopyToAsyncInternal(Stream stream)
        {
            await _response.Rewind().CopyToAsync(stream);
        }

        public override void Dispose()
        {
            _response.Dispose();
        }
    }

    public interface IHttpSession : ISessionScope
    {
        Stream Content { get; }

        Action<HttpRequestHeaders> ConfigureRequestHeaders { get; }

        MediaTypeFormatter RequestFormatter { get; }

        IEnumerable<MediaTypeFormatter> ResponseFormatters { get; }

        Type ResponseType { get; }

        string ContentType { get; }
    }
}