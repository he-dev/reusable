using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.IO.Extensions
{
    public static class FileInfoExtension
    {
        public static async Task<string> ReadAllTextAsync(this IFileInfo fileInfo, Encoding encoding)
        {
            using (var readStream = fileInfo.CreateReadStream())
            using (var streamReader = new StreamReader(readStream, encoding))
            {
                return await streamReader.ReadToEndAsync();
            }
        }

        public static Task<string> ReadAllTextAsync(this IFileInfo fileInfo)
        {
            return fileInfo.ReadAllTextAsync(Encoding.UTF8);
        }
    }
}
