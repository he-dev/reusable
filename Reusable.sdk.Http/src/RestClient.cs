using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Diagnostics;

namespace Reusable.sdk.Http
{
    [PublicAPI]
    public interface IRestClient
    {
        string BaseUri { get; }

        Task<T> InvokeAsync<T>([NotNull] HttpMethodContext context, CancellationToken cancellationToken);
    }

    // ReSharper disable once UnusedTypeParameter - This is a marker interface and it needs to stay.
    public interface IRestClient<TMarker> : IRestClient { }

    [PublicAPI]
    public class RestClient : IRestClient
    {
        private readonly Action<HttpRequestHeaders> _configureDefaultRequestHeaders;

        private readonly HttpClient _client;

        public RestClient(string baseUri, Action<HttpRequestHeaders> configureDefaultRequestHeaders)
        {
            _client = new HttpClient
            {
                BaseAddress = new Uri(baseUri)
            };
            _client.DefaultRequestHeaders.Accept.Clear();
            _configureDefaultRequestHeaders = configureDefaultRequestHeaders;
        }

        private string DebuggerDisplay() => this.ToDebuggerDisplayString(builder => { builder.DisplayMember(x => x.BaseUri); });

        public string BaseUri => _client.BaseAddress.ToString();

        public async Task<T> InvokeAsync<T>(HttpMethodContext context, CancellationToken cancellationToken = default)
        {
            var response = await SendRequestAsync(context, cancellationToken);

            if (context.EnsureSuccessStatusCode)
            {
                response.EnsureSuccessStatusCode();
            }

            var hasContent = response.Content.Headers.ContentLength > 0 && !(typeof(T) == typeof(object));
            if (hasContent)
            {
                return
                    await
                        response
                            .Content
                            .ReadAsAsync<T>(context.ResponseFormatters, cancellationToken)
                            .ContinueWith(t =>
                            {
                                response.Dispose();
                                return t.Result;
                            }, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                return
                    await
                        Task
                            .FromResult(default(T))
                            .ContinueWith(t =>
                            {
                                response.Dispose();
                                return default(T);
                            }, cancellationToken);
            }
        }

        private async Task<HttpResponseMessage> SendRequestAsync(HttpMethodContext context, CancellationToken cancellationToken)
        {
            using (var request = new HttpRequestMessage(context.Method, context.PartialUriBuilder.ToUri(_client.BaseAddress)))
            {
                if (!(context.Body is null))
                {
                    request.Content = new ObjectContent(context.Body.GetType(), context.Body, context.RequestFormatter);
                }

                _configureDefaultRequestHeaders(request.Headers);
                foreach (var configureRequestHeaders in context.RequestHeadersActions)
                {
                    configureRequestHeaders(request.Headers);
                }

                return await _client.SendAsync(request, cancellationToken);
            }
        }

        public static IRestClient<TMarker> Create<TMarker>(string baseUri, Action<HttpRequestHeaders> configureDefaultRequestHeaders)
        {
            return new RestClient<TMarker>(baseUri, configureDefaultRequestHeaders);
        }
    }

    internal class RestClient<TMarker> : RestClient, IRestClient<TMarker>
    {
        public RestClient(string baseUri, Action<HttpRequestHeaders> configureDefaultRequestHeaders)
            : base(baseUri, configureDefaultRequestHeaders)
        { }
    }

    public static class RestClientExtensions
    {
        [NotNull]
        public static IResource<TMarker> Resource<TMarker>([NotNull] this IRestClient<TMarker> client, params string[] path)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));

            return new Resource<TMarker>(client, path);
        }
    }
}