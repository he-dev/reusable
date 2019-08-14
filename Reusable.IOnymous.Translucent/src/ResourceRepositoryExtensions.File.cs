using System.IO;
using System.Text;
using System.Threading.Tasks;
using Reusable.Data;

namespace Reusable.IOnymous
{
    // Provides CRUD APIs.
    public static partial class ResourceRepositoryExtensions
    {
        // file:///

        public static async Task<IResource> GetFileAsync(this IResourceRepository resourceRepository, string path, MimeType format, IImmutableContainer properties = default)
        {
            return await resourceRepository.InvokeAsync(new Request.Get(CreateUri(path))
            {
                Context = properties.ThisOrEmpty().SetItem(ResourceProperties.Format, format)
            });
        }
        
        public static async Task<string> ReadTextFileAsync(this IResourceRepository resourceRepository, string path, IImmutableContainer metadata = default)
        {
            using (var file = await resourceRepository.GetFileAsync(path, MimeType.Plain, metadata))
            {
                return await file.DeserializeTextAsync();
            }
        }

        public static string ReadTextFile(this IResourceRepository resourceRepository, string path, IImmutableContainer metadata = default)
        {
            using (var file = resourceRepository.GetFileAsync(path, MimeType.Plain, metadata).GetAwaiter().GetResult())
            {
                return file.DeserializeTextAsync().GetAwaiter().GetResult();
            }
        }

        public static async Task<IResource> WriteTextFileAsync(this IResourceRepository resourceRepository, string path, string value, IImmutableContainer properties = default)
        {
            return await resourceRepository.InvokeAsync(new Request.Put(CreateUri(path))
            {
                Body = value,
                CreateBodyStreamCallback = body => ResourceHelper.SerializeTextAsync((string)body, properties.ThisOrEmpty().GetItemOrDefault(ResourceProperties.Encoding, Encoding.UTF8)),
                Context = properties.ThisOrEmpty().SetItem(ResourceProperties.Format, MimeType.Plain)
            });
        }

        public static async Task<IResource> WriteFileAsync(this IResourceRepository resourceRepository, string path, CreateStreamCallback createStream, IImmutableContainer context = default)
        {
            return await resourceRepository.InvokeAsync(new Request.Put(CreateUri(path))
            {
                // Body must not be null.
                Body = Body.Null,
                CreateBodyStreamCallback = createStream,
                Context = context.ThisOrEmpty()
            });
        }

        public static async Task<IResource> DeleteFileAsync(this IResourceRepository resourceRepository, string path, IImmutableContainer metadata = default)
        {
            return await resourceRepository.InvokeAsync(new Request.Delete(CreateUri(path))
            {
                Context = metadata.ThisOrEmpty().SetItem(ResourceProperties.Format, MimeType.Plain)
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