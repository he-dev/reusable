using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.sdk.Http
{
    public interface IResource<out TMarker>
    {
        [NotNull]
        IResource<TMarker> Configure([NotNull] Action<HttpMethodContext> configureContext);

        Task<TResult> GetAsync<TResult>(CancellationToken cancellationToken = default);

        Task<TResult> PutAsync<TResult>(CancellationToken cancellationToken = default);

        Task<TResult> PostAsync<TResult>(CancellationToken cancellationToken = default);

        Task<TResult> DeleteAsync<TResult>(CancellationToken cancellationToken = default);
    }

    [PublicAPI]
    internal class Resource<TMarker> : IResource<TMarker>
    {
        private Func<HttpMethodContext, HttpMethodContext> _configure;

        public Resource([NotNull] IRestClient<TMarker> client, params string[] path)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            PartialUriBuilder = new PartialUriBuilder(path);
        }

        [NotNull]
        private IRestClient<TMarker> Client { get; }

        [NotNull]
        public PartialUriBuilder PartialUriBuilder { get; }

        public IResource<TMarker> Configure(Action<HttpMethodContext> configureContext)
        {
            if (configureContext == null) throw new ArgumentNullException(nameof(configureContext));

            _configure = context =>
            {
                configureContext(context);
                return context;
            };

            return this;
        }

        public Task<TResult> GetAsync<TResult>(CancellationToken cancellationToken)
        {
            return Client.InvokeAsync<TResult>(_configure(new HttpMethodContext(HttpMethod.Get, PartialUriBuilder)), cancellationToken);
        }

        public Task<TResult> PutAsync<TResult>(CancellationToken cancellationToken)
        {
            return Client.InvokeAsync<TResult>(_configure(new HttpMethodContext(HttpMethod.Put, PartialUriBuilder)), cancellationToken);
        }

        public Task<TResult> PostAsync<TResult>(CancellationToken cancellationToken)
        {
            return Client.InvokeAsync<TResult>(_configure(new HttpMethodContext(HttpMethod.Post, PartialUriBuilder)), cancellationToken);
        }

        public Task<TResult> DeleteAsync<TResult>(CancellationToken cancellationToken)
        {
            return Client.InvokeAsync<TResult>(_configure(new HttpMethodContext(HttpMethod.Delete, PartialUriBuilder)), cancellationToken);
        }
    }

    public static class ResourceExtensions
    {
        public static Task<TResult> PostAsync<TMarker, TResult>(this IResource<TMarker> resource, TResult obj, CancellationToken cancellationToken = default)
        {
            return resource.PostAsync<TResult>(cancellationToken);
        }
    }
}