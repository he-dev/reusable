using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Diagnostics;

namespace Reusable.Net
{
    [PublicAPI]
    public interface IRestClient
    {
        string BaseUri { get; }

        Task<T> InvokeAsync<T>(HttpMethod httpMethod, UriDynamicPart uriDynamicPart, HttpMethodContext context, CancellationToken cancellationToken);
    }

    [PublicAPI]
    public class RestClient : IRestClient
    {
        private readonly HttpRequestHeadersConfiguration _defaultHttpRequestHeadersConfiguration;

        private readonly HttpClient _client;

        public RestClient(string baseUri, HttpRequestHeadersConfiguration defaultHttpRequestHeadersConfiguration)
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(baseUri)
            };
            _client.DefaultRequestHeaders.Accept.Clear();
            _defaultHttpRequestHeadersConfiguration = defaultHttpRequestHeadersConfiguration;
        }

        private string DebuggerDisplay() => this.ToDebuggerDisplayString(builder => { builder.Property(x => x.BaseUri); });

        public string BaseUri => _client.BaseAddress.ToString();

        public async Task<T> InvokeAsync<T>(HttpMethod httpMethod, UriDynamicPart uriDynamicPart, HttpMethodContext context, CancellationToken cancellationToken)
        {
            var response = await SendRequestAsync(httpMethod, uriDynamicPart, context, cancellationToken);

            var hasContent = response.Content.Headers.ContentLength > 0 && !(typeof(T) == typeof(object));
            if (hasContent)
            {
                return await response.Content
                    .ReadAsAsync<T>(new[] { context.ResponseFormatter }, cancellationToken)
                    .ContinueWith(t =>
                    {
                        response.Dispose();
                        return t.Result;
                    }, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                return await Task
                    .FromResult(default(T))
                    .ContinueWith(t =>
                    {
                        response.Dispose();
                        return default(T);
                    }, cancellationToken);
            }
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethod method, UriDynamicPart uriDynamicPart, HttpMethodContext context, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(method, CreateAbsoluteUri(uriDynamicPart)))
            {
                if (!(context.Body is null))
                {
                    request.Content = new ObjectContent(context.Body.GetType(), context.Body, context.RequestFormatter);
                }

                _defaultHttpRequestHeadersConfiguration.Apply(request.Headers);
                context.HttpRequestHeadersConfiguration.Apply(request.Headers);

                var response = await _client.SendAsync(request, cancellationToken);
                if (context.EnsureSuccessStatusCode)
                {
                    response.EnsureSuccessStatusCode();
                }

                return response;
            }
        }

        private Uri CreateAbsoluteUri(string uriDynamicPart)
        {
            return new Uri(_client.BaseAddress, uriDynamicPart);
        }
    }
}