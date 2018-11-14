using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Reusable.IO
{
    using static FileProviderCapabilities;

    public class CompositeFileProvider : IFileProvider
    {
        private readonly IEnumerable<IFileProvider> _fileProviders;

        public CompositeFileProvider(IEnumerable<IFileProvider> fileProviders)
        {
            _fileProviders = fileProviders;
        }

        public FileProviderCapabilities Capabilities => CanReadFile;

        public async Task<IFileInfo> GetFileInfoAsync(string path)
        {
            foreach (var fileProvider in _fileProviders)
            {
                var fileInfo = await fileProvider.GetFileInfoAsync(path);
                if (fileInfo.Exists)
                {
                    return fileInfo;
                }
            }

            return new InMemoryFileInfo(path, new byte[0]);
        }

        public Task<IFileInfo> CreateDirectoryAsync(string path)
        {
            throw new NotSupportedException($"{nameof(CompositeFileProvider)} does not support directory creation.");
        }

        public Task<IFileInfo> DeleteDirectoryAsync(string path, bool recursive)
        {
            throw new NotSupportedException($"{nameof(CompositeFileProvider)} does not support directory deletion.");
        }

        public Task<IFileInfo> SaveFileAsync(string path, Stream data)
        {
            throw new NotSupportedException($"{nameof(CompositeFileProvider)} does not support file creation.");
        }

        public Task<IFileInfo> DeleteFileAsync(string path)
        {
            throw new NotSupportedException($"{nameof(CompositeFileProvider)} does not support file deletion.");
        }
    }
}