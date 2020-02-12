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
using Reusable.Translucent.Annotations;
using Reusable.Translucent.Data;
using Reusable.Utilities.JsonNet.Converters;
using Reusable.Utilities.JsonNet.Extensions;

namespace Reusable.Translucent.Controllers
{
    [PublicAPI]
    [Handles(typeof(HttpRequest))]
    public class HttpController : ResourceController
    {
        private readonly HttpClient _client;

        public HttpController(ControllerName controllerName, HttpClient httpClient) : base(controllerName, httpClient.BaseAddress.ToString())
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
        public static HttpController FromBaseUri(ControllerName controllerName, string baseUri)
        {
            return new HttpController(controllerName, new HttpClient(new HttpClientHandler { UseProxy = false })
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
            using (var requestMessage = new HttpRequestMessage(method, uri))
            using (var content = (request.Body is {} body ? Serializer.Serialize(body) : Stream.Null))
            {
                if (content != Stream.Null)
                {
                    requestMessage.Content = new StreamContent(content.Rewind());
                    requestMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(request.ContentType);
                }

                foreach (var headerAction in HeaderActions.Concat(request.HeaderActions))
                {
                    headerAction(requestMessage.Headers);
                }

                using (var responseMessage = await _client.SendAsync(requestMessage, HttpCompletionOption.ResponseContentRead, request.CancellationToken).ConfigureAwait(false))
                {
                    var responseContentCopy = new MemoryStream();

                    if (responseMessage.Content.Headers.ContentLength > 0)
                    {
                        await responseMessage.Content.CopyToAsync(responseContentCopy);
                    }

                    switch (responseMessage.StatusCode.Class())
                    {
                        case HttpStatusCodeClass.Informational:
                        case HttpStatusCodeClass.Success: return OK<HttpResponse>(responseContentCopy, response => response.ContentType = responseMessage.Content.Headers.ContentType?.MediaType);
                        default: return NotFound<HttpResponse>(responseContentCopy, response => response.HttpStatusCode = (int)responseMessage.StatusCode);
                    }
                }
            }
        }

        public override void Dispose() => _client.Dispose();
    }
}