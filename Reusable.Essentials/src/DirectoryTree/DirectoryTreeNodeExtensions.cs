using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Reusable.Essentials.DirectoryTree;

public static class DirectoryTreeNodeExtensions
{
    /// <summary>
    /// Enumerates full directory names.
    /// </summary>
    public static IEnumerable<string> Directories(this IEnumerable<IDirectoryTreeNode> nodes, bool combine = true)
    {
        return nodes.Enumerate(node => node.DirectoryNames, combine);
    }

    /// <summary>
    /// Enumerates full file names.
    /// </summary>
    public static IEnumerable<string> Files(this IEnumerable<IDirectoryTreeNode> nodes, bool combine = true)
    {
        return nodes.Enumerate(node => node.FileNames, combine);
    }

    private static IEnumerable<string> Enumerate(this IEnumerable<IDirectoryTreeNode> nodes, Func<IDirectoryTreeNode, IEnumerable<string>> selector, bool combine)
    {
        return
            from node in nodes
            from name in selector(node)
            select combine ? node.Combine(node.DirectoryName, name) : name;
    }


    public static bool PhysicalDirectoryExists(this IDirectoryTreeNode? directoryTreeNode)
    {
        // Empty string does not exist and it'll return false.
        return Directory.Exists(directoryTreeNode?.DirectoryName ?? string.Empty);
    }
}