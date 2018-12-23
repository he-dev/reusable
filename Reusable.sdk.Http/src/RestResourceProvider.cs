using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Reusable.IOnymous;

namespace Reusable.sdk.Http
{
    public class RestResourceProvider : ResourceProvider
    {
        private readonly HttpClient _client;

        private readonly MethodInfo _invokeMethod;

        public RestResourceProvider(string baseUri, [NotNull] ResourceMetadata metadata)
            : base(new SoftString[] { "http", "https" }, metadata.SetItem(ResourceMetadataKeys.AllowRelativeUri, true))
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(baseUri)
            };
            _client.DefaultRequestHeaders.Clear();
        }

        public string BaseUri => _client.BaseAddress.ToString();

        //var invokeMethod = typeof(RestResourceProvider).GetMethod(nameof(InvokeAsync), BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { typeof(UriString), typeof(HttpMethod), typeof(ResourceMetadata) }, null);
        //var genericInvokeMethod = invokeMethod.MakeGenericMethod(metadata.ResponseType());
        //var response = await (Task<Stream>)genericInvokeMethod.Invoke(this, new object[] { uri, HttpMethod.Get, metadata });

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

    public static class ResourceProviderExtensions
    {
        
    }

    internal class RestResourceInfo : ResourceInfo
    {
        private readonly Stream _response;

        public RestResourceInfo([NotNull] UriString uri, Stream response)
            : base(uri, ResourceFormat.Json)
        {
            _response = response;
        }

        public override bool Exists => !(_response is null);

        public override long? Length { get; }

        public override DateTime? CreatedOn { get; }

        public override DateTime? ModifiedOn { get; }

        protected override async Task CopyToAsyncInternal(Stream stream)
        {
            _response.Seek(0, SeekOrigin.Begin);
            await _response.CopyToAsync(stream);
        }

//        protected override async Task<object> DeserializeAsyncInternal(Type targetType)
//        {
//            _response.Seek(0, SeekOrigin.Begin);
//            using (var streamReader = new StreamReader(_response))
//            {
//                return await streamReader.ReadToEndAsync();
//            }
//        }

        public override void Dispose()
        {
            _response.Dispose();
        }
    }

    [PublicAPI]
    public static class ResourceMetadataExtensions
    {
        public static Stream Content(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault<Stream>(nameof(Content));
        }

        public static ResourceMetadata Content(this ResourceMetadata metadata, Stream content)
        {
            return metadata.SetItem(nameof(Content), content);
        }

        public static Action<HttpRequestHeaders> ConfigureRequestHeaders(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault<Action<HttpRequestHeaders>>(nameof(ConfigureRequestHeaders), _ => { });
        }

        public static ResourceMetadata ConfigureRequestHeaders(this ResourceMetadata metadata, Action<HttpRequestHeaders> configureRequestHeaders)
        {
            return metadata.SetItem(nameof(configureRequestHeaders), configureRequestHeaders);
        }

        public static MediaTypeFormatter RequestFormatter(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault<MediaTypeFormatter>(nameof(RequestFormatter), new JsonMediaTypeFormatter());
        }

        public static ResourceMetadata RequestFormatter(this ResourceMetadata metadata, MediaTypeFormatter requestFormatter)
        {
            return metadata.SetItem(nameof(RequestFormatter), requestFormatter);
        }

        public static bool EnsureSuccessStatusCode(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(nameof(EnsureSuccessStatusCode), true);
        }

        public static ResourceMetadata EnsureSuccessStatusCode(this ResourceMetadata metadata, bool ensureSuccessStatusCode)
        {
            return metadata.SetItem(nameof(EnsureSuccessStatusCode), ensureSuccessStatusCode);
        }

        public static IEnumerable<MediaTypeFormatter> ResponseFormatters(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(nameof(ResponseFormatters), new MediaTypeFormatter[] { new JsonMediaTypeFormatter() });
        }

        public static ResourceMetadata ResponseFormatters(this ResourceMetadata metadata, params MediaTypeFormatter[] responseFormatters)
        {
            return metadata.SetItem(nameof(ResponseFormatters), (IEnumerable<MediaTypeFormatter>)responseFormatters);
        }

        public static Type ResponseType(this ResourceMetadata metadata)
        {
            return metadata.GetValueOrDefault(nameof(ResponseType), typeof(object));
        }

        public static ResourceMetadata ResponseType(this ResourceMetadata metadata, Type responseType)
        {
            return metadata.SetItem(nameof(ResponseType), responseType);
        }
    }
}