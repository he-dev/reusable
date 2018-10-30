using System.Collections.Generic;

namespace Reusable.IO
{
    internal class DirectoryTreeNodeFilter : IDirectoryTreeNode
    {
        internal DirectoryTreeNodeFilter(string path, IEnumerable<string> directoryNames, IEnumerable<string> fileNames)
        {
            DirectoryName = path;
            DirectoryNames = directoryNames;
            FileNames = fileNames;
        }

        public string DirectoryName { get; }

        public IEnumerable<string> DirectoryNames { get; }

        public IEnumerable<string> FileNames { get; }
    }
}