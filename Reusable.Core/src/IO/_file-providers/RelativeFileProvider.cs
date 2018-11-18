using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IO
{
    using static FileProviderCapabilities;

    public class RelativeFileProvider : IFileProvider
    {
        private readonly IFileProvider _fileProvider;

        private readonly string _basePath;

        public RelativeFileProvider([NotNull] IFileProvider fileProvider, [NotNull] string basePath)
        {
            _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        }

        public FileProviderCapabilities Capabilities => _fileProvider.Capabilities;

        public async Task<IFileInfo> GetFileInfoAsync(string path) => await _fileProvider.GetFileInfoAsync(CreateFullPath(path));

        public async Task<IFileInfo> CreateDirectoryAsync(string path) => await _fileProvider.CreateDirectoryAsync(CreateFullPath(path));

        public async Task<IFileInfo> DeleteDirectoryAsync(string path, bool recursive) => await _fileProvider.DeleteDirectoryAsync(CreateFullPath(path), recursive);

        public async Task<IFileInfo> SaveFileAsync(string path, Stream data) => await _fileProvider.SaveFileAsync(CreateFullPath(path), data);

        public async Task<IFileInfo> DeleteFileAsync(string path) => await _fileProvider.DeleteFileAsync(CreateFullPath(path));

        private string CreateFullPath(string path) => Path.Combine(_basePath, path ?? throw new ArgumentNullException(nameof(path)));
    }
}