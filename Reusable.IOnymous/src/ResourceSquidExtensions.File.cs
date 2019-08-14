using System.IO;
using System.Text;
using System.Threading.Tasks;
using Reusable.Data;

namespace Reusable.IOnymous
{
    // Provides CRUD APIs.
    public static partial class ResourceSquidExtensions
    {
        // file:///

        public static async Task<IResource> GetFileAsync(this IResourceSquid resourceSquid, string path, MimeType format, IImmutableContainer properties = default)
        {
            return await resourceSquid.InvokeAsync(new Request.Get(CreateUri(path))
            {
                Metadata = properties.ThisOrEmpty().SetItem(ResourceProperties.Format, format)
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

        public static async Task<IResource> WriteTextFileAsync(this IResourceSquid resourceSquid, string path, string value, IImmutableContainer properties = default)
        {
            return await resourceSquid.InvokeAsync(new Request.Put(CreateUri(path))
            {
                Body = value,
                CreateBodyStreamCallback = body => ResourceHelper.SerializeTextAsync((string)body, properties.ThisOrEmpty().GetItemOrDefault(ResourceProperties.Encoding, Encoding.UTF8)),
                Metadata = properties.ThisOrEmpty().SetItem(ResourceProperties.Format, MimeType.Plain)
            });
        }

        public static async Task<IResource> WriteFileAsync(this IResourceSquid resourceSquid, string path, CreateStreamCallback createStream, IImmutableContainer context = default)
        {
            return await resourceSquid.InvokeAsync(new Request.Put(CreateUri(path))
            {
                // Body must not be null.
                Body = Body.Null,
                CreateBodyStreamCallback = createStream,
                Metadata = context.ThisOrEmpty()
            });
        }

        public static async Task<IResource> DeleteFileAsync(this IResourceSquid resourceSquid, string path, IImmutableContainer metadata = default)
        {
            return await resourceSquid.InvokeAsync(new Request.Delete(CreateUri(path))
            {
                Metadata = metadata.ThisOrEmpty().SetItem(ResourceProperties.Format, MimeType.Plain)
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