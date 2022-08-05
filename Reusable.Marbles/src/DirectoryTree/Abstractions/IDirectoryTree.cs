using System;
using System.Collections.Generic;

namespace Reusable.Marbles.DirectoryTree;

public interface IDirectoryTree
{
    IEnumerable<IDirectoryTreeNode> Walk(string path, WalkOptions? options = default);

    Func<string, string, string> Combine { get; }
}