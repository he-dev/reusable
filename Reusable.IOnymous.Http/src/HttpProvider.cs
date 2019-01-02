using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionizer;

namespace Reusable.IOnymous
{
    public class HttpProvider : ResourceProvider
    {
        public new static readonly string DefaultScheme = "http";
        
        private readonly HttpClient _client;

        public HttpProvider([NotNull] string baseUri, ResourceMetadata metadata = default)
            : base(new SoftString[] { "http", "https" }, metadata.AllowRelativeUri(true))
        {
            if (baseUri == null) throw new ArgumentNullException(nameof(baseUri));
            
            _client = new HttpClient
            {
                BaseAddress = new Uri(baseUri)
            };
            _client.DefaultRequestHeaders.Clear();
        }

        public string BaseUri => _client.BaseAddress.ToString();

        protected override async Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata)
        {
            uri = BaseUri + uri;
            var (response, mediaType) = await InvokeAsync(uri, HttpMethod.Get, metadata);
            return new HttpResourceInfo(uri, response, new MimeType(mediaType));
        }

        protected override async Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata)
        {
            uri = BaseUri + uri;
            var (response, mediaType) = await InvokeAsync(uri, HttpMethod.Post, metadata.Scope<HttpProvider>(scope => scope.Content(value)));
            return new HttpResourceInfo(uri, response, new MimeType(mediaType));
        }

        #region Helpers

        private async Task<(Stream Content, string MimeType)> InvokeAsync(UriString uri, HttpMethod method, ResourceMetadata metadata)
        {
            using (var request = new HttpRequestMessage(method, uri))
            {
                var content = metadata.Scope<HttpProvider>().Content();
                if (content != null)
                {
                    request.Content = new StreamContent(content.Rewind());
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue(metadata.Scope<HttpProvider>().ContentType());
                }

                Metadata.Scope<HttpProvider>().ConfigureRequestHeaders()(request.Headers);
                metadata.Scope<HttpProvider>().ConfigureRequestHeaders()(request.Headers);
                using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, metadata.CancellationToken()))
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
            : base(uri, format)
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
}