using System;
using System.Collections.Generic;

namespace Reusable.Marbles.DirectoryTree;

public class RelativeDirectoryTree : IDirectoryTree, IDecorator<IDirectoryTree>
{
    public RelativeDirectoryTree(IDirectoryTree directoryTree, string directoryNameBase)
    {
        Decoratee = directoryTree;
        DirectoryNameBase = directoryNameBase;
    }

    public IDirectoryTree Decoratee { get; }

    public string DirectoryNameBase { get; }

    public Func<string, string, string> Combine => Decoratee.Combine;

    public IEnumerable<IDirectoryTreeNode> Walk(string path, WalkOptions? walkOptions = default)
    {
        return Decoratee.Walk(Combine(DirectoryNameBase, path), walkOptions);
    }
}