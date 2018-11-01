using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.IO.Extensions
{
    public static class FileInfoExtension
    {
        public static async Task<string> ReadAllTextAsync(this IFileInfo fileInfo, Encoding encoding)
        {
            if (!fileInfo.Exists)
            {
                throw new ArgumentException($"Cannot read file '{fileInfo.Path}' because it does not exist.");
            }

            using (var readStream = fileInfo.CreateReadStream())
            using (var streamReader = new StreamReader(readStream, encoding))
            {
                return await streamReader.ReadToEndAsync();
            }
        }

        public static Task<string> ReadAllTextAsync(this IFileInfo fileInfo) => fileInfo.ReadAllTextAsync(Encoding.UTF8);

        public static string ReadAllText(this IFileInfo fileInfo, Encoding encoding) => fileInfo.ReadAllTextAsync(encoding).GetAwaiter().GetResult();

        public static string ReadAllText(this IFileInfo fileInfo) => fileInfo.ReadAllTextAsync(Encoding.UTF8).GetAwaiter().GetResult();
    }
}
