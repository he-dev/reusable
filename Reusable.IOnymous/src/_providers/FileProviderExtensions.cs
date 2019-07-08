using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Quickey;

namespace Reusable.IOnymous
{
    public static class FileProviderExtensions
    {
        #region GET helpers

        public static async Task<IResource> ReadFileAsync(this IResourceProvider resourceProvider, string path, MimeType format, IImmutableContainer properties = default)
        {
            return await resourceProvider.GetAsync(CreateUri(path), properties.ThisOrEmpty().SetItem(Resource.Property.Format, format));
        }

//        public static IResource GetTextFile(this IResourceProvider resourceProvider, string path, IImmutableSession metadata = default)
//        {
//            return resourceProvider.ReadFileAsync(path, MimeType.Text, metadata).GetAwaiter().GetResult();
//        }

        public static async Task<string> ReadTextFileAsync(this IResourceProvider resourceProvider, string path, IImmutableContainer metadata = default)
        {
            using (var file = await resourceProvider.ReadFileAsync(path, MimeType.Text, metadata))
            {
                return await file.DeserializeTextAsync();
            }
        }

        public static string ReadTextFile(this IResourceProvider resourceProvider, string path, IImmutableContainer metadata = default)
        {
            using (var file = resourceProvider.ReadFileAsync(path, MimeType.Text, metadata).GetAwaiter().GetResult())
            {
                return file.DeserializeTextAsync().GetAwaiter().GetResult();
            }
        }

        #endregion

        #region PUT helpers

        public static async Task<IResource> WriteTextFileAsync(this IResourceProvider resourceProvider, string path, string value, IImmutableContainer properties = default)
        {
            return await resourceProvider.InvokeAsync(new Request
            {
                Uri = CreateUri(path),
                Method = RequestMethod.Put,
                CreateBodyStreamCallback = () => ResourceHelper.SerializeTextAsync(value, properties.ThisOrEmpty().GetItemOrDefault(Resource.Property.Encoding, Encoding.UTF8)),
                Context = properties.ThisOrEmpty().SetItem(Resource.Property.Format, MimeType.Text)
            });
        }

//        public static async Task<IResource> WriteFileAsync(this IResourceProvider resourceProvider, string path, Stream stream, IImmutableContainer properties = default)
//        {
//            return await resourceProvider.InvokeAsync(new Request
//            {
//                Uri = CreateUri(path),
//                Body = stream,
//                Properties = properties.ThisOrEmpty()
//            });
//        }

        #endregion

        #region POST helpers

        #endregion

        #region DELETE helpers

        public static async Task<IResource> DeleteFileAsync(this IResourceProvider resourceProvider, string path, IImmutableContainer metadata = default)
        {
            return await resourceProvider.InvokeAsync(new Request
            {
                Uri = CreateUri(path),
                Method = RequestMethod.Delete,
                Context = metadata.ThisOrEmpty().SetItem(Resource.Property.Format, MimeType.Text)
            });
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