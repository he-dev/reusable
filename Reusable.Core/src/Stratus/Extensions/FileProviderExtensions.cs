using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Reusable.Stratus.Extensions
{
    public static class FileProviderExtensions
    {
        public static async Task<IValueInfo> WriteValueAsync(this IValueProvider fileProvider, string path, string data, Encoding encoding = null)
        {
            using (var memoryStream = new MemoryStream((encoding ?? Encoding.UTF8).GetBytes(data)))
            {
                return await fileProvider.SerializeAsync(path, memoryStream);
            }
        }
    }
}
