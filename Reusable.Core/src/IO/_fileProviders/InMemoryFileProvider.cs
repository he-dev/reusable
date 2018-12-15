using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Reusable.IO
{
    using static FileProviderCapabilities;

    public class InMemoryFileProvider : Dictionary<string, byte[]>, IFileProvider
    {
        private readonly ISet<IFileInfo> _files = new HashSet<IFileInfo>();

        #region IFileProvider

        public FileProviderCapabilities Capabilities => CanCreateDirectory | CanDeleteDirectory | CanCreateFile | CanDeleteFile | CanReadFile;

        public Task<IFileInfo> GetFileInfoAsync(string path)
        {
            var file = _files.SingleOrDefault(f => FileInfoEqualityComparer.Default.Equals(f.Path, path));
            return Task.FromResult<IFileInfo>(file ?? new InMemoryFileInfo(path, default(byte[])));
        }

        public Task<IFileInfo> CreateDirectoryAsync(string path)
        {
            path = path.TrimEnd(Path.DirectorySeparatorChar);
            var newDirectory = new InMemoryFileInfo(path, _files.Where(f => f.Path.StartsWith(path)));
            _files.Add(newDirectory);
            return Task.FromResult<IFileInfo>(newDirectory);
        }

        public Task<IFileInfo> DeleteDirectoryAsync(string path, bool recursive)
        {
            return DeleteFileAsync(path);
        }

        public Task<IFileInfo> SaveFileAsync(string path, Stream data)
        {
            var file = new InMemoryFileInfo(path, GetByteArray(data));
            _files.Remove(file);
            _files.Add(file);
            return Task.FromResult<IFileInfo>(file);

            byte[] GetByteArray(Stream stream)
            {
                using (var memoryStream = new MemoryStream())
                {
                    stream.CopyTo(memoryStream);
                    return memoryStream.ToArray();
                }
            }
        }

        public Task<IFileInfo> DeleteFileAsync(string path)
        {
            var fileToDelete = new InMemoryFileInfo(path, default(byte[]));
            _files.Remove(fileToDelete);
            return Task.FromResult<IFileInfo>(fileToDelete);
        }

        #endregion
    }
}
