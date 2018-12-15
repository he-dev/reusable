using System.Text;

namespace Reusable.Stratus.Extensions
{
    public static class InMemoryFileProviderExtensions
    {
        /// <summary>
        /// Adds a file with the specified encoding.
        /// </summary>
        public static IValueInfo Add(this InMemoryValueProvider fileProvider, string path, string data, Encoding encoding)
        {
            return fileProvider.WriteValueAsync(path, data, encoding).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds a file with default encoding (UTF8).
        /// </summary>
        public static IValueInfo Add(this InMemoryValueProvider fileProvider, string path, string data)
        {
            return fileProvider.Add(path, data, Encoding.UTF8);
        }
    }
}