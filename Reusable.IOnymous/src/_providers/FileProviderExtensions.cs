using System.IO;
using System.Text;
using System.Threading.Tasks;
using Reusable.Data;

namespace Reusable.IOnymous
{
    public static class FileProviderExtensions
    {
        #region GET helpers

        public static async Task<IResourceInfo> GetFileAsync(this IResourceProvider resourceProvider, string path, MimeType format, IImmutableSession metadata = default)
        {
            return await resourceProvider.GetAsync(CreateUri(path), metadata.ThisOrEmpty().Set(Use<IResourceNamespace>.Namespace, x => x.Format, format));
        }

        public static IResourceInfo GetTextFile(this IResourceProvider resourceProvider, string path, IImmutableSession metadata = default)
        {
            return resourceProvider.GetFileAsync(path, MimeType.Text, metadata).GetAwaiter().GetResult();
        }

        public static async Task<string> ReadTextFileAsync(this IResourceProvider resourceProvider, string path, IImmutableSession metadata = default)
        {
            using (var file = await resourceProvider.GetFileAsync(path, MimeType.Text, metadata))
            {
                return await file.DeserializeTextAsync();
            }
        }

        public static string ReadTextFile(this IResourceProvider resourceProvider, string path, IImmutableSession metadata = default)
        {
            using (var file = resourceProvider.GetFileAsync(path, MimeType.Text, metadata).GetAwaiter().GetResult())
            {
                return file.DeserializeTextAsync().GetAwaiter().GetResult();
            }
        }

        #endregion

        #region PUT helpers

        public static async Task<IResourceInfo> WriteTextFileAsync(this IResourceProvider resourceProvider, string path, string value, IImmutableSession metadata = default)
        {
            using (var stream = await ResourceHelper.SerializeTextAsync(value, metadata.ThisOrEmpty().Get(Use<IAnyNamespace>.Namespace, x => x.Encoding, Encoding.UTF8)))
            {
                return await resourceProvider.PutAsync(CreateUri(path), stream, metadata.ThisOrEmpty().Set(Use<IResourceNamespace>.Namespace, x => x.Format, MimeType.Text));
            }
        }

        public static async Task<IResourceInfo> WriteFileAsync(this IResourceProvider resourceProvider, string path, Stream stream, IImmutableSession metadata = default)
        {
            return await resourceProvider.PutAsync(CreateUri(path), stream, metadata);
        }

        #endregion

        #region POST helpers

        #endregion

        #region DELETE helpers

        public static async Task<IResourceInfo> DeleteFileAsync(this IResourceProvider resourceProvider, string path, IImmutableSession metadata = default)
        {
            return await resourceProvider.DeleteAsync(CreateUri(path), metadata.ThisOrEmpty().Set(Use<IResourceNamespace>.Namespace, x => x.Format, MimeType.Text));
        }

        #endregion

        private static UriString CreateUri(string path)
        {
            return
                Path.IsPathRooted(path) || IsUnc(path)
                    ? new UriString(FileProvider.DefaultScheme, path)
                    : new UriString(path);
        }

        // https://www.pcmag.com/encyclopedia/term/53398/unc
        private static bool IsUnc(string value) => value.StartsWith("//");
    }
}