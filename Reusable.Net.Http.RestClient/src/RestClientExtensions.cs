using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Net.Http
{
    public static class RestClientExtensions
    {
        public static Task<T> GetAsync<T>(this IRestClient client, [NotNull] UriDynamicPart uriDynamicPart, [CanBeNull] HttpMethodContext context, CancellationToken cancellationToken)
        {
            return client.InvokeAsync<T>(HttpMethod.Get, uriDynamicPart, context, cancellationToken);
        }

        public static Task<T> PutAsync<T>(this IRestClient client, [NotNull] UriDynamicPart uriDynamicPart, [CanBeNull] HttpMethodContext context, CancellationToken cancellationToken)
        {
            return client.InvokeAsync<T>(HttpMethod.Put, uriDynamicPart, context, cancellationToken);
        }

        public static Task<T> PostAsync<T>(this IRestClient client, [NotNull] UriDynamicPart uriDynamicPart, [CanBeNull] HttpMethodContext context, CancellationToken cancellationToken)
        {
            return client.InvokeAsync<T>(HttpMethod.Post, uriDynamicPart, context, cancellationToken);
        }

        public static Task<T> DeleteAsync<T>(this IRestClient client, [NotNull] UriDynamicPart uriDynamicPart, [CanBeNull] HttpMethodContext context, CancellationToken cancellationToken)
        {
            return client.InvokeAsync<T>(HttpMethod.Delete, uriDynamicPart, context, cancellationToken);
        }
    }
}