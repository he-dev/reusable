using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

//using Eurowings.Utils;

namespace Reusable.IO
{
    public class InMemoryFileProvider : Dictionary<string, byte[]>, IFileProvider
    {
        private readonly ISet<IFileInfo> _files = new HashSet<IFileInfo>();

        #region IFileProvider

        public IFileInfo GetFileInfo(string path)
        {
            var file = _files.SingleOrDefault(f => FileInfoEqualityComparer.Default.Equals(f.Path, path));
            return file ?? new InMemoryFileInfo(path, default(byte[]));
        }

        public IFileInfo CreateDirectory(string path)
        {
            path = path.TrimEnd('\\');
            var newDirectory = new InMemoryFileInfo(path, _files.Where(f => f.Path.StartsWith(path)));
            _files.Add(newDirectory);
            return newDirectory;
        }

        public IFileInfo DeleteDirectory(string path, bool recursive)
        {
            return DeleteFile(path);

        }
        public Task<IFileInfo> CreateFileAsync(string path, Stream data)
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

        public IFileInfo DeleteFile(string path)
        {
            var fileToDelete = new InMemoryFileInfo(path, default(byte[]));
            _files.Remove(fileToDelete);
            return fileToDelete;
        }

        #endregion
    }
}
