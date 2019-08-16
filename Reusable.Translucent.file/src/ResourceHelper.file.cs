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

        public static async Task<Response> GetFileAsync(this IResourceSquid resourceSquid, string path, IImmutableContainer properties = default)
        {
            return await resourceSquid.InvokeAsync(new Request.Get(CreateUri(path))
            {
                //ContentType = format
                //Metadata = properties.ThisOrEmpty().SetItem(Request.Accept, format)
            });
        }

        public static async Task<string> ReadTextFileAsync(this IResourceSquid resourceSquid, string path, IImmutableContainer metadata = default)
        {
            using (var file = await resourceSquid.GetFileAsync(path, metadata))
            {
                return await file.DeserializeTextAsync();
            }
        }

        public static string ReadTextFile(this IResourceSquid resourceSquid, string path, IImmutableContainer metadata = default)
        {
            using (var file = resourceSquid.GetFileAsync(path, metadata).GetAwaiter().GetResult())
            {
                return file.DeserializeTextAsync().GetAwaiter().GetResult();
            }
        }

        public static async Task WriteTextFileAsync(this IResourceSquid resourceSquid, string path, string value, IImmutableContainer properties = default)
        {
            using (await resourceSquid.InvokeAsync(new Request.Put(CreateUri(path))
            {
                Body = value,
            })) { }
        }

        public static async Task WriteFileAsync(this IResourceSquid resourceSquid, string path, CreateBodyStreamDelegate createBodyStream, IImmutableContainer context = default)
        {
            using (await resourceSquid.InvokeAsync(new Request.Put(CreateUri(path))
            {
                Body = createBodyStream,
                Metadata = context.ThisOrEmpty()
            })) { }
        }

        public static async Task DeleteFileAsync(this IResourceSquid resourceSquid, string path, IImmutableContainer metadata = default)
        {
            await resourceSquid.InvokeAsync(new Request.Delete(CreateUri(path))
            {
                //ContentType = MimeType.Plain
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