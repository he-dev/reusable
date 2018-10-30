using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;

namespace Reusable.IO
{
    public static class DirectoryTreeNodeExtensions
    {
        public static void Deconstruct(
            [CanBeNull] this IDirectoryTreeNode directoryTreeNode,
            [CanBeNull] out string directoryName,
            [CanBeNull] out IEnumerable<string> directoryNames,
            [CanBeNull] out IEnumerable<string> fileNames)
        {
            directoryName = directoryTreeNode?.DirectoryName;
            directoryNames = directoryTreeNode?.DirectoryNames;
            fileNames = directoryTreeNode?.FileNames;
        }

        public static bool Exists(
            [CanBeNull] this IDirectoryTreeNode directoryTreeNode)
        {
            // Empty string does not exist and it'll return false.
            return Directory.Exists(directoryTreeNode?.DirectoryName ?? string.Empty);
        }
    }
}
