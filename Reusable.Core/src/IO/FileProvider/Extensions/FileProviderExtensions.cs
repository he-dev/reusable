using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.IO.Extensions
{
    public static class FileProviderExtensions
    {
        public static async Task<IFileInfo> CreateFileAsync(this IFileProvider fileProvider, string path, string data, Encoding encoding = null, bool canOverwrite = false)
        {
            using (var memoryStream = new MemoryStream((encoding ?? Encoding.UTF8).GetBytes(data)))
            {
                return await fileProvider.CreateFileAsync(path, memoryStream);
            }
        }        
    }
}
