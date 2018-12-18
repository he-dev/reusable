using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.IOnymous.Extensions
{
    public static class FileProviderExtensions
    {
        public static async Task<IResourceInfo> WriteValueAsync(this IResourceProvider fileProvider, string path, string data, Encoding encoding = null)
        {
            using (var memoryStream = new MemoryStream((encoding ?? Encoding.UTF8).GetBytes(data)))
            {
                return await fileProvider.PutAsync(path, memoryStream);
            }
        }
    }
}
