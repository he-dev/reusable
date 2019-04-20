using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Exceptionize;

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

        public static async Task<T> GetItemAsync<T>(this IResourceProvider resources, UriString uri, Metadata metadata = default)
        {
            using (var item = await resources.GetAsync(uri, metadata))
            {
                if (item.Exists)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await item.CopyToAsync(memoryStream);

                        if (item.Metadata.Resource().Format() == MimeType.Text)
                        {
                            using (var streamReader = new StreamReader(memoryStream.Rewind()))
                            {
                                return (T)(object)await streamReader.ReadToEndAsync();
                            }
                        }

                        if (item.Metadata.Resource().Format() == MimeType.Binary)
                        {
                            return (T)await ResourceHelper.DeserializeBinaryAsync<object>(memoryStream.Rewind());
                        }
                    }

                    throw DynamicException.Create
                    (
                        $"ItemFormat",
                        $"Item's '{uri}' format is '{item.Metadata.Resource().Format()}' but only '{MimeType.Binary}' and '{MimeType.Text}' are supported."
                    );
                }
                else
                {
                    throw DynamicException.Create
                    (
                        $"ItemNotFound",
                        $"Could not find '{uri}' that maps to '{item.Metadata.Resource().InternalName() ?? "N/A"}'."
                    );
                }
            }
        }

        public static Task<IResourceInfo> GetAnyAsync(this IResourceProvider resourceProvider, UriString uri, Metadata metadata = default)
        {
            return resourceProvider.GetAsync(uri.With(x => x.Scheme, ResourceProvider.DefaultScheme), metadata);
        }

        #endregion

        #region PUT helpers

        public static async Task SetItemAsync(this IResourceProvider resources, UriString uri, object value, Metadata metadata = default)
        {
            if (metadata.Resource().Type() == typeof(string))
            {
                using (var stream = await ResourceHelper.SerializeTextAsync((string)value))
                using (await resources.PutAsync(uri, stream, metadata.Resource(s => s.Format(MimeType.Text)))) { }
            }
            else
            {
                using (var stream = await ResourceHelper.SerializeBinaryAsync(value))
                using (await resources.PutAsync(uri, stream, metadata.Resource(s => s.Format(MimeType.Binary)))) { }
            }
        }

        #endregion

        #region POST helpers        

        public static async Task<IResourceInfo> PostAsync
        (
            this IResourceProvider resourceProvider,
            UriString uri,
            Func<Task<Stream>> serializeAsync,
            Metadata metadata = default
        )
        {
            return await ExecuteAsync(resourceProvider.PostAsync, uri, serializeAsync, metadata);
        }

        public static async Task<IResourceInfo> PutAsync
        (
            this IResourceProvider resourceProvider,
            UriString uri,
            Func<Task<Stream>> serializeAsync,
            Metadata metadata = default
        )
        {
            return await ExecuteAsync(resourceProvider.PutAsync, uri, serializeAsync, metadata);
        }

        private static async Task<IResourceInfo> ExecuteAsync
        (
            Func<UriString, Stream, Metadata, Task<IResourceInfo>> executeAsync,
            UriString uri,
            Func<Task<Stream>> serializeAsync,
            Metadata metadata
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