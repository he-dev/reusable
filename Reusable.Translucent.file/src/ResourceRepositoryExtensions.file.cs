using System.IO;
using System.Text;
using System.Threading.Tasks;
using Reusable.Data;
using Reusable.Translucent.Controllers;

namespace Reusable.Translucent
{
    public static class FileResourceHelper
    {
        // file:///

        public static Task<Response> GetFileAsync(this IResourceRepository resources, string path, IImmutableContainer? metadata = default)
        {
            return resources.GetAsync(CreateUri(path), default, metadata.ThisOrEmpty().SetItem(ResourceController.Schemes, UriSchemes.Known.File));
        }

        public static async Task<string> ReadTextFileAsync(this IResourceRepository resourceRepository, string path, IImmutableContainer? metadata = default)
        {
            using var file = await resourceRepository.GetFileAsync(path, metadata);
            return await file.DeserializeTextAsync();
        }

        public static string ReadTextFile(this IResourceRepository resources, string path, IImmutableContainer? metadata = default)
        {
            using var file = resources.GetFileAsync(path, metadata).GetAwaiter().GetResult();
            return file.DeserializeTextAsync().GetAwaiter().GetResult();
        }

        public static async Task WriteTextFileAsync(this IResourceRepository resources, string path, string value, IImmutableContainer? metadata = default)
        {
            using (await resources.PutAsync(CreateUri(path), value, metadata.ThisOrEmpty().SetItem(ResourceController.Schemes, UriSchemes.Known.File))) { }
        }

        public static async Task WriteFileAsync(this IResourceRepository resources, string path, CreateBodyStreamDelegate createBodyStream, IImmutableContainer? metadata = default)
        {
            using (await resources.PutAsync(CreateUri(path), createBodyStream, metadata.ThisOrEmpty().SetItem(ResourceController.Schemes, UriSchemes.Known.File))) { }
        }

        public static async Task DeleteFileAsync(this IResourceRepository resources, string path, IImmutableContainer metadata = default)
        {
            using (await resources.DeleteAsync(CreateUri(path), default, metadata.ThisOrEmpty().SetItem(ResourceController.Schemes, UriSchemes.Known.File))) { }
        }

        private static UriString CreateUri(string path)
        {
            return
                Path.IsPathRooted(path) || IsUnc(path)
                    ? new UriString(UriSchemes.Known.File, path)
                    : new UriString(path);
        }

        // https://www.pcmag.com/encyclopedia/term/53398/unc
        private static bool IsUnc(string value) => value.StartsWith("//");
    }
}