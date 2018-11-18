using System;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Reusable.sdk.Http;

namespace Reusable.sdk.Mailr
{
    public interface IMailrClient : IRestClient { }

    public class MailrClient : IMailrClient
    {
        private readonly IRestClient _restClient;

        private MailrClient(IRestClient restClient)
        {
            _restClient = restClient;
        }

        public string BaseUri => _restClient.BaseUri;

        public static IMailrClient Create(string baseUri, Action<HttpRequestHeaders> configureDefaultRequestHeaders)
        {
            var restClient = new RestClient(baseUri, configureDefaultRequestHeaders);
            return new MailrClient(restClient);
        }

        public Task<T> InvokeAsync<T>(HttpMethodContext context, CancellationToken cancellationToken)
        {
            return _restClient.InvokeAsync<T>(context, cancellationToken);
        }
    }    
}