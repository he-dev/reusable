using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IO
{
    using static FileProviderCapabilities;

    [PublicAPI]
    public class PhysicalFileProvider : IFileProvider
    {
        public FileProviderCapabilities Capabilities => CanCreateDirectory | CanDeleteDirectory | CanCreateFile | CanDeleteFile | CanReadFile;

        public Task<IFileInfo> GetFileInfoAsync(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            return Task.FromResult<IFileInfo>(new PhysicalFileInfo(path));
        }

        public Task<IFileInfo> CreateDirectoryAsync(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));


            try
            {
                if (Directory.Exists(path))
                {
                    return Task.FromResult<IFileInfo>(new PhysicalFileInfo(path));
                }

                var newDirectory = Directory.CreateDirectory(path);
                return Task.FromResult<IFileInfo>(new PhysicalFileInfo(newDirectory.FullName));
            }
            catch (Exception ex)
            {
                throw new CreateDirectoryException(path, ex);
            }
        }

        public async Task<IFileInfo> SaveFileAsync(string path, Stream data)
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
                throw new SaveFileException(path, ex);
            }
        }

        public Task<IFileInfo> DeleteFileAsync(string path)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));

            try
            {
                File.Delete(path);
                return Task.FromResult<IFileInfo>(new PhysicalFileInfo(path));
            }
            catch (Exception ex)
            {
                throw new DeleteFileException(path, ex);
            }
        }

        public Task<IFileInfo> DeleteDirectoryAsync(string path, bool recursive)
        {
            try
            {
                Directory.Delete(path, recursive);
                return Task.FromResult<IFileInfo>(new PhysicalFileInfo(path));
            }
            catch (Exception ex)
            {
                throw new DeleteDirectoryException(path, ex);
            }
        }
    }
}
