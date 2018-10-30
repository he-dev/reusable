using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;

namespace Reusable.IO
{
    [PublicAPI]
    public interface IDirectoryTreeNode
    {
        [NotNull]
        string DirectoryName { get; }

        [NotNull, ItemNotNull]
        IEnumerable<string> DirectoryNames { get; }

        [NotNull, ItemNotNull]
        IEnumerable<string> FileNames { get; }
    }

    internal class DirectoryTreeNode : IDirectoryTreeNode
    {
        internal DirectoryTreeNode(string path)
        {
            DirectoryName = path;
        }

        public string DirectoryName { get; }

        public IEnumerable<string> DirectoryNames => Directory.EnumerateDirectories(DirectoryName).Select(Path.GetFileName);

        public IEnumerable<string> FileNames => Directory.EnumerateFiles(DirectoryName).Select(Path.GetFileName);
    }
}