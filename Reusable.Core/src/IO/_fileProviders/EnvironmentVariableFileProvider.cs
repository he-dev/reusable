using System;
using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IO
{
    public partial class EnvironmentVariableFileProvider : IFileProvider
    {
        private readonly IFileProvider _fileProvider;

        public EnvironmentVariableFileProvider([NotNull] IFileProvider fileProvider)
        {
            _fileProvider = fileProvider ?? throw new ArgumentNullException(nameof(fileProvider));
        }

        public FileProviderCapabilities Capabilities => _fileProvider.Capabilities;

        public Task<IFileInfo> GetFileInfoAsync(string path) => _fileProvider.GetFileInfoAsync(Environment.ExpandEnvironmentVariables(path));

        public Task<IFileInfo> CreateDirectoryAsync(string path) => _fileProvider.CreateDirectoryAsync(Environment.ExpandEnvironmentVariables(path));

        public Task<IFileInfo> DeleteDirectoryAsync(string path, bool recursive) =>  _fileProvider.DeleteDirectoryAsync(Environment.ExpandEnvironmentVariables(path), recursive);

        public Task<IFileInfo> SaveFileAsync(string path, Stream data) => _fileProvider.SaveFileAsync(Environment.ExpandEnvironmentVariables(path), data);

        public Task<IFileInfo> DeleteFileAsync(string path) => _fileProvider.DeleteFileAsync(Environment.ExpandEnvironmentVariables(path));
    }
}