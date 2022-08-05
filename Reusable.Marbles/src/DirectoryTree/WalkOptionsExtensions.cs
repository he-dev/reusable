using System;
using JetBrains.Annotations;

namespace Reusable.Marbles.DirectoryTree;

[PublicAPI]
public static class WalkOptionsExtensions
{
    public static WalkOptions SuppressExceptions(this WalkOptions options)
    {
        return options.Also(x => x.Catch = (node, ex) => { });
    }

    public static WalkOptions Where(this WalkOptions options, Func<IDirectoryTreeNode, bool> predicate)
    {
        return options.Also(x => x.Predicate = predicate);
    }

    public static WalkOptions MaxDepth(this WalkOptions options, int maxDepth)
    {
        return options.Where(node => node.Depth < maxDepth);
    }

    public static WalkOptions OnException(this WalkOptions options, Action<IDirectoryTreeNode, Exception> onException)
    {
        return options.Also(node => node.Catch = onException);
    }
}