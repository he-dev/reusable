using System;
using System.Collections.Generic;

namespace Reusable.IO.DirectoryTree
{
    public interface IDirectoryTree
    {
        IEnumerable<IDirectoryTreeNode> Walk(string path, WalkOptions? options = default);

        Func<string, string, string> Combine { get; }
    }
}