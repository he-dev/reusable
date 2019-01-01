using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Newtonsoft.Json;

namespace Reusable.IOnymous
{
    public static class FileProviderExtensions
    {
        #region GET helpers

        public static async Task<IResourceInfo> GetFileAsync(this IResourceProvider resourceProvider, string path, MimeType format, ResourceMetadata metadata = null)
        {
            var uri = Path.IsPathRooted(path) ? new UriString(FileProvider.DefaultScheme, path) : new UriString(path);
            return await resourceProvider.GetAsync(uri, (metadata ?? ResourceMetadata.Empty).Format(format));
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
            using (var stream = await ResourceHelper.SerializeAsTextAsync(value, metadata.Encoding()))
            {
                var uri = Path.IsPathRooted(path) ? new UriString(FileProvider.DefaultScheme, path) : new UriString(path);
                return await resourceProvider.PutAsync(uri, stream, (metadata ?? ResourceMetadata.Empty).Format(MimeType.Text));
            }
        }

        public static async Task<IResourceInfo> SaveFileAsync(this IResourceProvider resourceProvider, string path, Stream stream, ResourceMetadata metadata = null)
        {
            var uri = Path.IsPathRooted(path) ? new UriString(FileProvider.DefaultScheme, path) : new UriString(path);
            return await resourceProvider.PutAsync(uri, stream, metadata);
        }

        #endregion

        #region POST helpers

        #endregion

        #region DELETE helpers

        public static async Task<IResourceInfo> DeleteFileAsync(this IResourceProvider resourceProvider, string path, ResourceMetadata metadata = null)
        {
            var uri = Path.IsPathRooted(path) ? new UriString(FileProvider.DefaultScheme, path) : new UriString(path);
            return await resourceProvider.DeleteAsync(uri, (metadata ?? ResourceMetadata.Empty).Format(MimeType.Text));
        }

        #endregion
    }
}