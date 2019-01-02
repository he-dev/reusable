using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.IOnymous
{
    public static class ResourceProviderExtensions
    {
        [Pure]
        [NotNull]
        public static TDecorator DecorateWith<TDecorable, TDecorator>(this TDecorable decorable, Func<TDecorable, TDecorator> decoratorFactory)
            where TDecorable : IResourceProvider
            where TDecorator : IResourceProvider
        {
            return decoratorFactory(decorable);
        }

        #region GET helpers

        public static Task<IResourceInfo> GetAnyAsync(this IResourceProvider resourceProvider, UriString uri, ResourceMetadata metadata = default)
        {
            return resourceProvider.GetAsync(uri.With(x => x.Scheme, ResourceProvider.DefaultScheme), metadata);
        }

        #endregion

        #region PUT helpers

        #endregion

        #region POST helpers        

        public static async Task<IResourceInfo> PostAsync
        (
            this IResourceProvider resourceProvider,
            UriString uri,
            Func<Task<Stream>> serializeAsync,
            ResourceMetadata metadata = default
        )
        {
            return await ExecuteAsync(resourceProvider.PostAsync, uri, serializeAsync, metadata);
        }

        public static async Task<IResourceInfo> PutAsync
        (
            this IResourceProvider resourceProvider,
            UriString uri,
            Func<Task<Stream>> serializeAsync,
            ResourceMetadata metadata = default
        )
        {
            return await ExecuteAsync(resourceProvider.PutAsync, uri, serializeAsync, metadata);
        }

        private static async Task<IResourceInfo> ExecuteAsync
        (
            Func<UriString, Stream, ResourceMetadata, Task<IResourceInfo>> executeAsync,
            UriString uri,
            Func<Task<Stream>> serializeAsync,
            ResourceMetadata metadata
        )
        {
            var stream = await serializeAsync();
            var post = executeAsync(uri, stream, metadata);
            await post.ContinueWith(_ => stream.Dispose());

            return await post;
        }

        #endregion

        #region DELETE helpers

        #endregion
    }
}