using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.Net
{
    public static class ResourceContextFactory
    {
        public static IResourceContext<TResource> ToResource<TResource>(this IRestClient client, string name = null)
        {
            // We get the resource name either from the attribute or the name of the interface without the "I" prefix.
            var resourceName =
                name 
                ?? typeof(TResource)
                    .GetCustomAttribute<ResourceNameAttribute>()
                    ?.ToString()
                ?? Regex.Replace(typeof(TResource).Name, "^I", string.Empty);

            return (IResourceContext<TResource>)Activator.CreateInstance(typeof(ResourceContext<TResource>), new object[] { client, resourceName });
        }
    }

    // ReSharper disable once UnusedTypeParameter - The generic argument is used for building strong extensions.
    public interface IResourceContext<TResource>
    {
        IRestClient Client { get; }

        UriDynamicPart UriDynamicPart { get; }

        HttpMethodContext MethodContext { get; }

        Task<TResult> GetAsync<TResult>(CancellationToken cancellationToken);

        Task<TResult> PutAsync<TResult>(CancellationToken cancellationToken);

        Task<TResult> PostAsync<TResult>(CancellationToken cancellationToken);

        Task<TResult> DeleteAsync<TResult>(CancellationToken cancellationToken);
    }

    [PublicAPI]
    public class ResourceContext<TResource> : IResourceContext<TResource>
    {
        public ResourceContext([NotNull] IRestClient client, params string[] path)
        {
            Client = client ?? throw new ArgumentNullException(nameof(client));
            UriDynamicPart = new UriDynamicPart(path);
        }

        [NotNull]
        public IRestClient Client { get; }

        public UriDynamicPart UriDynamicPart { get; }

        public HttpMethodContext MethodContext { get; } = new HttpMethodContext();

        public Task<TResult> GetAsync<TResult>(CancellationToken cancellationToken)
        {
            return Client.GetAsync<TResult>(UriDynamicPart, MethodContext, cancellationToken);
        }

        public Task<TResult> PutAsync<TResult>(CancellationToken cancellationToken)
        {
            return Client.PutAsync<TResult>(UriDynamicPart, MethodContext, cancellationToken);
        }

        public Task<TResult> PostAsync<TResult>(CancellationToken cancellationToken)
        {
            return Client.PostAsync<TResult>(UriDynamicPart, MethodContext, cancellationToken);
        }

        public Task<TResult> DeleteAsync<TResult>(CancellationToken cancellationToken)
        {
            return Client.DeleteAsync<TResult>(UriDynamicPart, MethodContext, cancellationToken);
        }
    }

    public static class ResourceContextExtensions
    {
        public static IResourceContext<TResource> SetHeader<TResource>(this IResourceContext<TResource> resource, string header, params string[] values)
        {
            resource.MethodContext.HttpRequestHeadersConfiguration += headers =>
            {
                headers.Remove(header);
                headers.Add(header, values);
            };
            return resource;
        }
    }

    [UsedImplicitly]
    [AttributeUsage(AttributeTargets.Interface)]
    public class ResourceNameAttribute : Attribute
    {
        private readonly string _resourceName;

        public ResourceNameAttribute([NotNull] string resourceName)
        {
            _resourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
        }

        public override string ToString()
        {
            return _resourceName;
        }
    }

    internal static class Chain
    {
        [NotNull]
        public static Action<T> Append<T>([NotNull] this Action<T> left, [NotNull] Action<T> right)
        {
            if (left == null) throw new ArgumentNullException(nameof(left));
            if (right == null) throw new ArgumentNullException(nameof(right));

            return obj =>
            {
                left(obj);
                right(obj);
            };
        }
    }

    //public static class StringExtensions
    //{
    //    public static string ToResourceName(this string memberName) => Regex.Replace(memberName, "^(Get|Update)|(RequestBuilder|Async)$", string.Empty);
    //}
}