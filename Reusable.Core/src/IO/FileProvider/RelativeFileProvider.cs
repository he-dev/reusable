using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IO
{
    public class RelativeFileProvider : IFileProvider
    {
        private readonly IFileProvider _fileProvider;

        private readonly string _basePath;

        public RelativeFileProvider([NotNull] IFileProvider fileProvider, [NotNull] string basePath)
        {
            _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        }

        public IFileInfo GetFileInfo(string path) => _fileProvider.GetFileInfo(CreateFullPath(path));

        public IFileInfo CreateDirectory(string path) => _fileProvider.CreateDirectory(CreateFullPath(path));

        public IFileInfo DeleteDirectory(string path, bool recursive) => _fileProvider.DeleteDirectory(CreateFullPath(path), recursive);

        public Task<IFileInfo> CreateFileAsync(string path, Stream data) => _fileProvider.CreateFileAsync(CreateFullPath(path), data);

        public IFileInfo DeleteFile(string path) => _fileProvider.DeleteFile(CreateFullPath(path));

        private string CreateFullPath(string path) => Path.Combine(_basePath, path ?? throw new ArgumentNullException(nameof(path)));
    }
}