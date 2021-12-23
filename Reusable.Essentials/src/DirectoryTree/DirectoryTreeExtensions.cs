using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Essentials.DirectoryTree;

public static class DirectoryTreeExtensions
{
    public static IEnumerable<IDirectoryTreeNode> WhereDirectory(this IEnumerable<IDirectoryTreeNode> nodes, Func<string, bool> predicate)
    {
        return
            from node in nodes
            select new DirectoryTreeNode
            (
                node.DirectoryName,
                node.Depth,
                from directoryName in node.DirectoryNames where predicate(node.Combine(node.DirectoryName, directoryName)) select directoryName,
                node.FileNames,
                node.Combine
            );
    }

    public static IEnumerable<IDirectoryTreeNode> WhereFile(this IEnumerable<IDirectoryTreeNode> nodes, Func<string, bool> predicate)
    {
        return
            from node in nodes
            select new DirectoryTreeNode
            (
                node.DirectoryName,
                node.Depth,
                node.DirectoryNames,
                from fileName in node.FileNames where predicate(node.Combine(node.DirectoryName, fileName)) select fileName,
                node.Combine
            );
    }
}