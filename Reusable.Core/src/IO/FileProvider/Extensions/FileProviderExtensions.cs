using System.IO;
using System.Text;
using System.Threading.Tasks;
using Reusable.Validation;

namespace Reusable.IO.Extensions
{
    using FPC = FileProviderCapabilities;

    public static class FileProviderExtensions
    {
        public static async Task<IFileInfo> CreateFileAsync(this IFileProvider fileProvider, string path, string data, Encoding encoding = null, bool canOverwrite = false)
        {
            using (var memoryStream = new MemoryStream((encoding ?? Encoding.UTF8).GetBytes(data)))
            {
                return await fileProvider.CreateFileAsync(path, memoryStream);
            }
        }

        public static bool CanCreateDirectory(this IFileProvider fileProvider) => fileProvider.Capabilities.HasFlag(FPC.CanCreateDirectory);
        public static bool CanDeleteDirectory(this IFileProvider fileProvider) => fileProvider.Capabilities.HasFlag(FPC.CanDeleteDirectory);
        public static bool CanCreateFile(this IFileProvider fileProvider) => fileProvider.Capabilities.HasFlag(FPC.CanCreateFile);
        public static bool CanDeleteFile(this IFileProvider fileProvider) => fileProvider.Capabilities.HasFlag(FPC.CanDeleteFile);
        public static bool CanReadFile(this IFileProvider fileProvider) => fileProvider.Capabilities.HasFlag(FPC.CanReadFile);
    }
}
