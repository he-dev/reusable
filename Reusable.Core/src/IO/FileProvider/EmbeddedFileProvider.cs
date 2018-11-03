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
    public class EmbeddedFileProvider : IFileProvider
    {
        private readonly Assembly _assembly;

        public EmbeddedFileProvider([NotNull] Assembly assembly)
        {
            _assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            BasePath = _assembly.GetName().Name.Replace('.', '\\');
        }

        public string BasePath { get; }

        public IFileInfo GetFileInfo(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            // Embedded resouce names are separated by '.' so replace the windows separator.
            var fullName = Path.Combine(BasePath, path).Replace('\\', '.');

            // Embedded resource names are case sensitive so find the actual name of the resource.
            var actualName = _assembly.GetManifestResourceNames().FirstOrDefault(name => SoftString.Comparer.Equals(name, fullName));
            var getManifestResourceStream = actualName is null ? default(Func<Stream>) : () => _assembly.GetManifestResourceStream(actualName);

            return new EmbeddedFileInfo(UndoConvertPath(fullName), getManifestResourceStream);
        }

        // Convert path back to windows format but the last '.' - this is the file extension.
        private static string UndoConvertPath(string path) => Regex.Replace(path, @"\.(?=.*?\.)", "\\");

        public IFileInfo CreateDirectory(string path)
        {
            throw new NotSupportedException($"{nameof(EmbeddedFileProvider)} does not support directory creation.");
        }

        public IFileInfo DeleteDirectory(string path, bool recursive)
        {
            throw new NotSupportedException($"{nameof(EmbeddedFileProvider)} does not support directory deletion.");
        }

        public Task<IFileInfo> CreateFileAsync(string path, Stream data)
        {
            throw new NotSupportedException($"{nameof(EmbeddedFileProvider)} does not support file creation.");
        }

        public IFileInfo DeleteFile(string path)
        {
            throw new NotSupportedException($"{nameof(EmbeddedFileProvider)} does not support file deletion.");
        }
    }

    public static class EmbeddedFileProvider<T>
    {
        public static IFileProvider Default { get; } = new EmbeddedFileProvider(typeof(T).Assembly);
    }
}