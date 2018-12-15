using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Stratus.Extensions
{
    public static class FileInfoExtension
    {
        public static async Task<string> ToStringAsync(this IValueInfo value, Encoding encoding)
        {
            if (!value.Exists)
            {
                throw new ArgumentException($"Cannot read file '{value.Name}' because it does not exist.");
            }

            using (var memoryStream = new MemoryStream())
            {
                await value.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);
                using (var streamReader = new StreamReader(memoryStream, encoding))
                {
                    return await streamReader.ReadToEndAsync();
                }
            }
        }

        public static Task<string> ToStringAsync(this IValueInfo value) => value.ToStringAsync(Encoding.UTF8);

        public static string ToString(this IValueInfo fileInfo, Encoding encoding) => fileInfo.ToStringAsync(encoding).GetAwaiter().GetResult();

        public static string ToString(this IValueInfo fileInfo) => fileInfo.ToStringAsync(Encoding.UTF8).GetAwaiter().GetResult();
    }
}
