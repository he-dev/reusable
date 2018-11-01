using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IO
{
    [PublicAPI]
    public class PhysicalFileProvider : IFileProvider
    {
        public IFileInfo GetFileInfo(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            return new PhysicalFileInfo(path);
        }

        public IFileInfo CreateDirectory(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            if (Directory.Exists(path))
            {
                return new PhysicalFileInfo(path);
            }

            try
            {
                var newDirectory = Directory.CreateDirectory(path);
                return new PhysicalFileInfo(newDirectory.FullName);
            }
            catch (Exception ex)
            {
                throw new CreateDirectoryException(path, ex);
            }
        }

        public async Task<IFileInfo> CreateFileAsync(string path, Stream data)
        {
            try
            {
                using (var fileStream = new FileStream(path, FileMode.CreateNew, FileAccess.Write))
                {
                    await data.CopyToAsync(fileStream);
                    await fileStream.FlushAsync();
                }
                return new PhysicalFileInfo(path);
            }
            catch (Exception ex)
            {
                throw new CreateFileException(path, ex);
            }
        }

        public IFileInfo DeleteFile(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            try
            {
                File.Delete(path);
                return new PhysicalFileInfo(path);
            }
            catch (Exception ex)
            {
                throw new DeleteFileException(path, ex);
            }
        }

        public IFileInfo DeleteDirectory(string path, bool recursive)
        {
            try
            {
                Directory.Delete(path, recursive);
                return new PhysicalFileInfo(path);
            }
            catch (Exception ex)
            {
                throw new DeleteDirectoryException(path, ex);
            }
        }
    }
}
