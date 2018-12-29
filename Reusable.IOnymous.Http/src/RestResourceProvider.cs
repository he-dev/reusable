using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IOnymous
{
    public class RestResourceProvider : ResourceProvider
    {
        private readonly HttpClient _client;

        public RestResourceProvider(string baseUri, ResourceMetadata metadata = null)
            : base(new SoftString[] { "http", "https" }, metadata.AllowRelativeUri(true))
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(baseUri)
            };
            _client.DefaultRequestHeaders.Clear();
        }

        public string BaseUri => _client.BaseAddress.ToString();

        protected override async Task<IResourceInfo> GetAsyncInternal(UriString uri, ResourceMetadata metadata = null)
        {
            uri = BaseUri + uri;
            var response = await InvokeAsync(uri, HttpMethod.Get, metadata);
            return new RestResourceInfo(uri, response);
        }

        protected override async Task<IResourceInfo> PostAsyncInternal(UriString uri, Stream value, ResourceMetadata metadata = null)
        {
            uri = BaseUri + uri;
            var response = await InvokeAsync(uri, HttpMethod.Post, (metadata ?? ResourceMetadata.Empty).Content(value));
            return new RestResourceInfo(uri, response);
        }

        #region Helpers

        private async Task<Stream> InvokeAsync(UriString uri, HttpMethod method, ResourceMetadata metadata)
        {
            using (var request = new HttpRequestMessage(method, uri))
            {
                var content = metadata.Content();
                if (content != null)
                {
                    content.Seek(0, SeekOrigin.Begin);
                    request.Content = new StreamContent(content);
                    request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                }

                Metadata.ConfigureRequestHeaders()(request.Headers);
                metadata.ConfigureRequestHeaders()(request.Headers);
                using (var response = await _client.SendAsync(request, HttpCompletionOption.ResponseContentRead, metadata.CancellationToken()))
                {
                    if (metadata.EnsureSuccessStatusCode())
                    {
                        response.EnsureSuccessStatusCode();
                    }

                    if (response.Content.Headers.ContentLength > 0)
                    {
                        var responseContentCopy = new MemoryStream();
                        await response.Content.CopyToAsync(responseContentCopy);
                        return responseContentCopy;
                    }

                    return null;
                }
            }
        }

        #endregion

        public override void Dispose()
        {
            _client.Dispose();
        }
    }

    internal class RestResourceInfo : ResourceInfo
    {
        private readonly Stream _response;

        public RestResourceInfo([NotNull] UriString uri, Stream response)
            : base(uri, MimeType.Json)
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