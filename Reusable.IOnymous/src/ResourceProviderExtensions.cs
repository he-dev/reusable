using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Data;
using Reusable.Exceptionize;
using Reusable.Quickey;

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

        public static async Task<T> GetItemAsync<T>(this IResourceProvider resources, UriString uri, IImmutableSession metadata = default)
        {
            using (var item = await resources.GetAsync(uri, metadata))
            {
                var itemFormat = item.Metadata.GetItemOrDefault(From<IResourceMeta>.Select(x => x.Format));

                if (item.Exists)
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        await item.CopyToAsync(memoryStream);

                        if (itemFormat == MimeType.Text)
                        {
                            using (var streamReader = new StreamReader(memoryStream.Rewind()))
                            {
                                return (T)(object)await streamReader.ReadToEndAsync();
                            }
                        }

                        if (itemFormat == MimeType.Binary)
                        {
                            return (T)await ResourceHelper.DeserializeBinaryAsync<object>(memoryStream.Rewind());
                        }
                    }

                    throw DynamicException.Create
                    (
                        $"ItemFormat",
                        $"Item's '{uri}' format is '{itemFormat}' but only '{MimeType.Binary}' and '{MimeType.Text}' are supported."
                    );
                }
                else
                {
                    throw DynamicException.Create
                    (
                        $"ItemNotFound",
                        $"Could not find '{uri}' that maps to '{item.Metadata.GetItemOrDefault(From<IResourceMeta>.Select(x => x.ActualName)) ?? "N/A"}'."
                    );
                }
            }
        }

        public static T GetItem<T>(this IResourceProvider resources, UriString uri, IImmutableSession metadata = default)
        {
            return resources.GetItemAsync<T>(uri, metadata).GetAwaiter().GetResult();
        }

        public static Task<IResource> GetAnyAsync(this IResourceProvider resourceProvider, UriString uri, IImmutableSession metadata = default)
        {
            return resourceProvider.GetAsync(uri.With(x => x.Scheme, ResourceSchemes.IOnymous), metadata);
        }

        #endregion

        #region PUT helpers

        public static async Task SetItemAsync(this IResourceProvider resources, UriString uri, object value, IImmutableSession metadata)
        {
            if (metadata.GetItemOrDefault(From<IResourceMeta>.Select(x => x.Type)) == typeof(string))
            {
                using (var stream = await ResourceHelper.SerializeTextAsync((string)value))
                using (await resources.PutAsync(uri, stream, metadata.SetItem(From<IResourceMeta>.Select(x => x.Format), MimeType.Text))) { }
            }
            else
            {
                using (var stream = await ResourceHelper.SerializeBinaryAsync(value))
                using (await resources.PutAsync(uri, stream, metadata.SetItem(From<IResourceMeta>.Select(x => x.Format), MimeType.Binary))) { }
            }
        }

        #endregion

        #region POST helpers        

        public static async Task<IResource> PostAsync
        (
            this IResourceProvider resourceProvider,
            UriString uri,
            Func<Task<Stream>> serializeAsync,
            IImmutableSession metadata = default
        )
        {
            return await ExecuteAsync(resourceProvider.PostAsync, uri, serializeAsync, metadata);
        }

        public static async Task<IResource> PutAsync
        (
            this IResourceProvider resourceProvider,
            UriString uri,
            Func<Task<Stream>> serializeAsync,
            IImmutableSession metadata = default
        )
        {
            return await ExecuteAsync(resourceProvider.PutAsync, uri, serializeAsync, metadata);
        }

        private static async Task<IResource> ExecuteAsync
        (
            Func<UriString, Stream, IImmutableSession, Task<IResource>> executeAsync,
            UriString uri,
            Func<Task<Stream>> serializeAsync,
            IImmutableSession metadata
        )
        {
            var stream = await serializeAsync();
            var post = executeAsync(uri, stream, metadata);
            await post.ContinueWith(t =>
            {
                stream.Dispose();
                if (t.IsFaulted && t.Exception != null)
                {
                    throw t.Exception;
                }
            });

            return await post;
        }

        #endregion

        #region DELETE helpers

        #endregion

        private static readonly Action<ResourceRequest> Pass = _ => { };

        public static async Task<IResource> GetAsync(this IResourceProvider resources, UriString uri, Action<ResourceRequest> configure = default)
        {
            var request = new ResourceRequest { Method = ResourceRequestMethod.Get, Uri = uri };
            (configure ?? Pass)(request);
            return await resources.RequestAsync(request);
        }
    }
}