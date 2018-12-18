using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.IOnymous.Extensions
{
    public static class FileInfoExtension
    {
        public static async Task<string> ToStringAsync(this IResourceInfo resource, Encoding encoding)
        {
            if (!resource.Exists)
            {
                throw new ArgumentException($"Cannot read file '{resource.Uri}' because it does not exist.");
            }

            using (var memoryStream = new MemoryStream())
            {
                await resource.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var streamReader = new StreamReader(memoryStream, encoding))
                {
                    return await streamReader.ReadToEndAsync();
                }
            }
        }

        public static Task<string> ToStringAsync(this IResourceInfo resource) => resource.ToStringAsync(Encoding.UTF8);

        public static string ToString(this IResourceInfo fileInfo, Encoding encoding) => fileInfo.ToStringAsync(encoding).GetAwaiter().GetResult();

        public static string ToString(this IResourceInfo fileInfo) => fileInfo.ToStringAsync(Encoding.UTF8).GetAwaiter().GetResult();
    }
}
