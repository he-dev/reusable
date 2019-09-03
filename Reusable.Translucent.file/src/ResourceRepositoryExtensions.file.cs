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

        public static async Task<Response> GetFileAsync(this IResourceRepository resourceRepository, string path, IImmutableContainer properties = default)
        {
            return await resourceRepository.InvokeAsync(new Request.Get(CreateUri(path))
            {
                //ContentType = format
                Metadata = properties.ThisOrEmpty()
            });
        }

        public static async Task<string> ReadTextFileAsync(this IResourceRepository resourceRepository, string path, IImmutableContainer metadata = default)
        {
            using (var file = await resourceRepository.GetFileAsync(path, metadata))
            {
                return await file.DeserializeTextAsync();
            }
        }

        public static string ReadTextFile(this IResourceRepository resourceRepository, string path, IImmutableContainer metadata = default)
        {
            using (var file = resourceRepository.GetFileAsync(path, metadata).GetAwaiter().GetResult())
            {
                return file.DeserializeTextAsync().GetAwaiter().GetResult();
            }
        }

        public static async Task WriteTextFileAsync(this IResourceRepository resourceRepository, string path, string value, IImmutableContainer properties = default)
        {
            using (await resourceRepository.InvokeAsync(new Request.Put(CreateUri(path))
            {
                Body = value,
                Metadata = properties.ThisOrEmpty()
            })) { }
        }

        public static async Task WriteFileAsync(this IResourceRepository resourceRepository, string path, CreateBodyStreamDelegate createBodyStream, IImmutableContainer context = default)
        {
            using (await resourceRepository.InvokeAsync(new Request.Put(CreateUri(path))
            {
                Body = createBodyStream,
                Metadata = context.ThisOrEmpty()
            })) { }
        }

        public static async Task DeleteFileAsync(this IResourceRepository resourceRepository, string path, IImmutableContainer metadata = default)
        {
            await resourceRepository.InvokeAsync(new Request.Delete(CreateUri(path))
            {
                //ContentType = MimeType.Plain
                Metadata = metadata.ThisOrEmpty()
            });
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