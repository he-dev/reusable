using System.IO;
using System.Linq;

namespace Reusable.Marbles.DirectoryTree;

internal class PhysicalDirectoryTreeNode : DirectoryTreeNode
{
    internal PhysicalDirectoryTreeNode(string directoryName, int depth = 0) : base
    (
        directoryName,
        depth,
        Directory.EnumerateDirectories(directoryName).Select(Path.GetFileName),
        Directory.EnumerateFiles(directoryName).Select(Path.GetFileName),
        Path.Combine
    ) { }
}