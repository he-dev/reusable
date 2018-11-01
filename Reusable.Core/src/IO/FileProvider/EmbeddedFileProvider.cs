using System;
using System.IO;
using System.Linq;
using System.Reflection;
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
        }

        public IFileInfo GetFileInfo(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            // Embedded resouce names are separated by '.' so replace the windows separator.
            var name = path.Replace("\\", ".");

            // Embedded resource name are case sensitive so find the actual name of the resource.
            name =
                _assembly
                    .GetManifestResourceNames()
                    .FirstOrDefault(n => n.Equals(name, StringComparison.OrdinalIgnoreCase));

            var getManifestResourceStream =
                name is null
                    ? default(Func<Stream>)
                    : () => _assembly.GetManifestResourceStream(name);

            return new EmbeddedFileInfo(path, getManifestResourceStream);
        }

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
}