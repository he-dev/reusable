using System.IO;
using System.Text;
using System.Threading.Tasks;
using Reusable.Data;

namespace Reusable.Translucent
{
    public static class ResourceSquidExtensions
    {
        // file:///

        public static async Task<Response> GetFileAsync(this IResourceSquid resourceSquid, string path, MimeType format, IImmutableContainer properties = default)
        {
            return await resourceSquid.InvokeAsync(new Request.Get(CreateUri(path))
            {
                ContentType = format
            });
        }
        
        public static async Task<string> ReadTextFileAsync(this IResourceSquid resourceSquid, string path, IImmutableContainer metadata = default)
        {
            using (var file = await resourceSquid.GetFileAsync(path, MimeType.Plain, metadata))
            {
                return await file.DeserializeTextAsync();
            }
        }

        public static string ReadTextFile(this IResourceSquid resourceSquid, string path, IImmutableContainer metadata = default)
        {
            using (var file = resourceSquid.GetFileAsync(path, MimeType.Plain, metadata).GetAwaiter().GetResult())
            {
                return file.DeserializeTextAsync().GetAwaiter().GetResult();
            }
        }

        public static async Task<Response> WriteTextFileAsync(this IResourceSquid resourceSquid, string path, string value, IImmutableContainer properties = default)
        {
            return await resourceSquid.InvokeAsync(new Request.Put(CreateUri(path))
            {
                Body = value,
                CreateBodyStreamCallback = body => ResourceHelper.SerializeTextAsync((string)body, properties.ThisOrEmpty().GetItemOrDefault(ResponseProperties.Encoding, Encoding.UTF8)),
                ContentType = MimeType.Plain
            });
        }

        public static async Task<Response> WriteFileAsync(this IResourceSquid resourceSquid, string path, CreateStreamCallback createStream, IImmutableContainer context = default)
        {
            return await resourceSquid.InvokeAsync(new Request.Put(CreateUri(path))
            {
                // Body must not be null.
                Body = Body.Null,
                CreateBodyStreamCallback = createStream,
                Metadata = context.ThisOrEmpty()
            });
        }

        public static async Task<Response> DeleteFileAsync(this IResourceSquid resourceSquid, string path, IImmutableContainer metadata = default)
        {
            return await resourceSquid.InvokeAsync(new Request.Delete(CreateUri(path))
            {
                ContentType = MimeType.Plain
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