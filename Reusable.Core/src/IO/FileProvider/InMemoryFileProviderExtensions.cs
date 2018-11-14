using System.Text;
using Reusable.IO.Extensions;

namespace Reusable.IO
{
    public static class InMemoryFileProviderExtensions
    {
        /// <summary>
        /// Adds a file with the specified encoding.
        /// </summary>
        public static IFileInfo Add(this InMemoryFileProvider fileProvider, string path, string data, Encoding encoding)
        {
            return fileProvider.CreateFileAsync(path, data, encoding).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a file with default encoding (UTF8).
        /// </summary>
        public static IFileInfo Add(this InMemoryFileProvider fileProvider, string path, string data)
        {
            return fileProvider.Add(path, data, Encoding.UTF8);
        }

        /// <summary>
        /// Adds a directory.
        /// </summary>
        public static void Add(this InMemoryFileProvider fileProvider, string path)
        {
            fileProvider.CreateDirectoryAsync(path).GetAwaiter().GetResult();
        }
    }
}