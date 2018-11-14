using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IO
{
    using static FileProviderCapabilities;

    public class EmbeddedFileProvider : IFileProvider
    {
        private readonly Assembly _assembly;

        public EmbeddedFileProvider([NotNull] Assembly assembly)
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            BasePath = _assembly.GetName().Name.Replace('.', Path.DirectorySeparatorChar);
        }

        public string BasePath { get; }

        #region IFileProvider

        public FileProviderCapabilities Capabilities => CanReadFile;

        public Task<IFileInfo> GetFileInfoAsync(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            // Embedded resouce names are separated by '.' so replace the windows separator.
            var fullName = Path.Combine(BasePath, path).Replace(Path.DirectorySeparatorChar, '.');

            // Embedded resource names are case sensitive so find the actual name of the resource.
            var actualName = _assembly.GetManifestResourceNames().FirstOrDefault(name => SoftString.Comparer.Equals(name, fullName));
            var getManifestResourceStream = actualName is null ? default(Func<Stream>) : () => _assembly.GetManifestResourceStream(actualName);

            return Task.FromResult<IFileInfo>(new EmbeddedFileInfo(UndoConvertPath(fullName), getManifestResourceStream));
        }

        public Task<IFileInfo> CreateDirectoryAsync(string path)
        {
            throw new NotSupportedException($"{nameof(EmbeddedFileProvider)} does not support directory creation.");
        }

        public Task<IFileInfo> DeleteDirectoryAsync(string path, bool recursive)
        {
            throw new NotSupportedException($"{nameof(EmbeddedFileProvider)} does not support directory deletion.");
        }

        public Task<IFileInfo> CreateFileAsync(string path, Stream data)
        {
            throw new NotSupportedException($"{nameof(EmbeddedFileProvider)} does not support file creation.");
        }

        public Task<IFileInfo> DeleteFileAsync(string path)
        {
            throw new NotSupportedException($"{nameof(EmbeddedFileProvider)} does not support file deletion.");
        }
        
        #endregion
        
        // Convert path back to windows format but the last '.' - this is the file extension.
        private static string UndoConvertPath(string path) => Regex.Replace(path, @"\.(?=.*?\.)", Path.DirectorySeparatorChar.ToString());
    }

    public static class EmbeddedFileProvider<T>
    {
        public static IFileProvider Default { get; } = new EmbeddedFileProvider(typeof(T).Assembly);
    }
}