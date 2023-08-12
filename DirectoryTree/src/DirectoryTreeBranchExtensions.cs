using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reusable.IO.Abstractions;

namespace Reusable.IO;

public static class DirectoryTreeBranchExtensions
{
    public static IEnumerable<DirectoryTree.Item> Directories(this IDirectoryTreeBranch branch)
    {
        return
            from path in Directory.EnumerateDirectories(branch.Path)
            select new DirectoryTree.Item(branch.Root, path, Path.GetDirectoryName(path));
    }

    public static IEnumerable<DirectoryTree.Item> Files(this IDirectoryTreeBranch branch)
    {
        return
            from path in Directory.EnumerateFiles(branch.Path)
            select new DirectoryTree.Item(branch.Root, path, Path.GetFileName(path));
    }
}