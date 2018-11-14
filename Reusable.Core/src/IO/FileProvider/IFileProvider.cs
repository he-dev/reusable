using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IO
{
    [PublicAPI]
    public interface IFileProvider
    {
        FileProviderCapabilities Capabilities { get; }

        /// <summary>
        /// Gets file or directory info.
        /// </summary>
        Task<IFileInfo> GetFileInfoAsync([NotNull] string path);

        Task<IFileInfo> CreateDirectoryAsync([NotNull] string path);

        Task<IFileInfo> DeleteDirectoryAsync([NotNull] string path, bool recursive);

        Task<IFileInfo> CreateFileAsync([NotNull] string path, [NotNull] Stream data);

        Task<IFileInfo> DeleteFileAsync([NotNull] string path);
    }

    [Flags]
    public enum FileProviderCapabilities
    {
        None = 0,

        CanCreateDirectory = 1 << 0,
        CanDeleteDirectory = 1 << 1,
        CanCreateFile = 1 << 2,
        CanDeleteFile = 1 << 3,
        CanReadFile = 1 << 4
    }
}
