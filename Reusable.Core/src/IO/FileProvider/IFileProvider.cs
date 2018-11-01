using System.IO;
using System.Threading.Tasks;
using JetBrains.Annotations;

namespace Reusable.IO
{
    [PublicAPI]
    public interface IFileProvider
    {
        /// <summary>
        /// Gets file or directory info.
        /// </summary>
        [NotNull]
        IFileInfo GetFileInfo([NotNull] string path);

        [NotNull]
        IFileInfo CreateDirectory([NotNull] string path);

        [NotNull]
        IFileInfo DeleteDirectory([NotNull] string path, bool recursive);

        [NotNull]
        Task<IFileInfo> CreateFileAsync([NotNull] string path, [NotNull] Stream data);

        [NotNull]
        IFileInfo DeleteFile([NotNull] string path);                
    }
}
