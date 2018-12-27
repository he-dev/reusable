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
        public static IResourceProvider DecorateWith(this IResourceProvider decorable, Func<IResourceProvider, IResourceProvider> decoratorFactory)
        {
            return decoratorFactory(decorable);
        }

        #region GET helpers

        public static async Task<IResourceInfo> GetFileAsync(this IResourceProvider resourceProvider, string path, MimeType format, ResourceMetadata metadata = null)
        {
            var uri = Path.IsPathRooted(path) ? new UriString(PhysicalFileProvider.Scheme, path) : new UriString(path);
            return await resourceProvider.GetAsync(uri, (metadata ?? ResourceMetadata.Empty).Format(format));
        }

        public static Task<IResourceInfo> GetAnyAsync(this IResourceProvider resourceProvider, UriString uri, ResourceMetadata metadata = null)
        {
            return resourceProvider.GetAsync(uri.With(x => x.Scheme, ResourceProvider.DefaultScheme), metadata);
        }

        public static Task<IResourceInfo> GetHttpAsync(this IResourceProvider resourceProvider, string path, ResourceMetadata metadata = null)
        {
            var uri = new UriString("http", path);
            return resourceProvider.GetAsync(uri, metadata);
        }

        public static async Task<string> ReadTextFileAsync(this IResourceProvider resourceProvider, string path, ResourceMetadata metadata = null)
        {
            var file = await resourceProvider.GetFileAsync(path, MimeType.Text, metadata);
            return await file.DeserializeTextAsync();
        }

        public static string ReadTextFile(this IResourceProvider resourceProvider, string path, ResourceMetadata metadata = null)
        {
            var file = resourceProvider.GetFileAsync(path, MimeType.Text, metadata).GetAwaiter().GetResult();
            return file.DeserializeTextAsync().GetAwaiter().GetResult();
        }

        #endregion

        #region PUT helpers

        public static async Task<IResourceInfo> WriteTextFileAsync(this IResourceProvider resourceProvider, string path, string value, ResourceMetadata metadata = null)
        {
            using (var stream = await ResourceHelper.SerializeAsTextAsync(value, metadata.GetValueOrDefault<Encoding>(nameof(Encoding))))
            {
                var uri = Path.IsPathRooted(path) ? new UriString(PhysicalFileProvider.Scheme, path) : new UriString(path);
                return await resourceProvider.PutAsync(uri, stream, (metadata ?? ResourceMetadata.Empty).Format(MimeType.Text));
            }
        }

        #endregion

        #region POST helpers

        public static async Task<IResourceInfo> PostJsonAsync<T>(this IResourceProvider resourceProvider, UriString uri, T obj, ResourceMetadata metadata = null)
        {
            var memoryStream = new MemoryStream();
            var textWriter = new StreamWriter(memoryStream);
            var jsonWriter = new JsonTextWriter(textWriter);
            metadata.JsonSerializer().Serialize(jsonWriter, obj);
            jsonWriter.Flush();

            var post = resourceProvider.PostAsync(uri, memoryStream, metadata);

            await post.ContinueWith(_ =>
            {
                (jsonWriter as IDisposable).Dispose();
                textWriter.Dispose();
                memoryStream.Dispose();
            });

            return await post;
        }

        public static async Task<IResourceInfo> PostAsync<T>(this IResourceProvider resourceProvider, UriString uri, T obj, Func<T, Task<Stream>> serializeAsync, ResourceMetadata metadata = null)
        {
            var stream = await serializeAsync(obj);
            var post = resourceProvider.PostAsync(uri, stream, metadata);
            await post.ContinueWith(_ => stream?.Dispose());

            return await post;
        }

        #endregion
    }
}