using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Reusable.Extensions;
using Reusable.Translucent.Abstractions;
using Reusable.Translucent.Data;
using Reusable.Utilities.JsonNet.Converters;
using Reusable.Utilities.JsonNet.Extensions;

namespace Reusable.Translucent.Controllers
{
    [PublicAPI]
    public class HttpController : ResourceController<HttpRequest>
    {
        private readonly HttpClient _client;

        public HttpController(HttpClient httpClient) : base(httpClient.BaseAddress.ToString())
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

        public List<Action<HttpRequestHeaders>> HeaderActions { get; set; } = new List<Action<HttpRequestHeaders>>();

        /// <summary>
        /// Create a HttpProvider that doesn't use a proxy for requests.
        /// </summary>
        public static HttpController FromBaseUri(string baseUri)
        {
            return new HttpController(new HttpClient(new HttpClientHandler { UseProxy = false })
            {
                BaseAddress = new Uri(baseUri)
            });
        }

        public override async Task<Response> ReadAsync(HttpRequest request) => await InvokeAsync(HttpMethod.Get, request);

        public override async Task<Response> UpdateAsync(HttpRequest request) => await InvokeAsync(HttpMethod.Put, request);

        public override async Task<Response> CreateAsync(HttpRequest request) => await InvokeAsync(HttpMethod.Post, request);

        public override async Task<Response> DeleteAsync(HttpRequest request) => await InvokeAsync(HttpMethod.Delete, request);

        private async Task<Response> InvokeAsync(HttpMethod method, HttpRequest request)
        {
            var uri = BaseUri is {} baseUri ? Path.Combine(baseUri, request.ResourceName) : request.ResourceName;

            using var requestMessage = new HttpRequestMessage(method, uri);
            using var content = (request.Body is {} body ? Serializer.Serialize(body) : Stream.Null);

            if (content != Stream.Null)
            {
                requestMessage.Content = new StreamContent(content.Rewind());
                requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(request.ContentType);
            }

            foreach (var headerAction in HeaderActions.Concat(request.HeaderActions))
            {
                headerAction(requestMessage.Headers);
            }

            using var responseMessage = await _client.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead, request.CancellationToken).ConfigureAwait(false);

            var responseContentCopy = new MemoryStream();

            if (responseMessage.Content.Headers.ContentLength > 0)
            {
                await responseMessage.Content.CopyToAsync(responseContentCopy);
            }

            var contentType = responseMessage.Content.Headers.ContentType?.MediaType;
            var statusCode = responseMessage.StatusCode;

            return statusCode.Class() switch
            {
                HttpStatusCodeClass.Informational => Success<HttpResponse>(request.ResourceName, responseContentCopy, response => response.ContentType = contentType),
                HttpStatusCodeClass.Success => Success<HttpResponse>(request.ResourceName, responseContentCopy, response => response.ContentType = contentType),
                _ => NotFound<HttpResponse>(request.ResourceName, responseContentCopy, response => { response.HttpStatusCode = (int)statusCode; })
            };
        }

        public override void Dispose() => _client.Dispose();
    }
}