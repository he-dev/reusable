using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Reusable.IO
{
    public class CompositeFileProvider : IFileProvider
    {
        private readonly IEnumerable<IFileProvider> _fileProviders;

        public CompositeFileProvider(IEnumerable<IFileProvider> fileProviders)
        {
            _fileProviders = fileProviders;
        }

        public IFileInfo GetFileInfo(string path)
        {
            foreach (var fileProvider in _fileProviders)
            {
                var fileInfo = fileProvider.GetFileInfo(path);
                if (fileInfo.Exists)
                {
                    return fileInfo;
                }
            }

            return new InMemoryFileInfo(path, new byte[0]);
        }

        public IFileInfo CreateDirectory(string path)
        {
            throw new NotSupportedException($"{nameof(CompositeFileProvider)} does not support directory creation.");
        }

        public IFileInfo DeleteDirectory(string path, bool recursive)
        {
            throw new NotSupportedException($"{nameof(CompositeFileProvider)} does not support directory deletion.");
        }

        public Task<IFileInfo> CreateFileAsync(string path, Stream data)
        {
            throw new NotSupportedException($"{nameof(CompositeFileProvider)} does not support file creation.");
        }

        public IFileInfo DeleteFile(string path)
        {
            throw new NotSupportedException($"{nameof(CompositeFileProvider)} does not support file deletion.");
        }
    }
}