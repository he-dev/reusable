using System;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.IO.DirectoryTree
{
    [PublicAPI]
    public static class WalkOptionsExtensions
    {
        public static WalkOptions SuppressExceptions(this WalkOptions options)
        {
            return options.Pipe(x => x.Catch = (node, ex) => { });
        }

        public static WalkOptions Where(this WalkOptions options, Func<IDirectoryTreeNode, bool> predicate)
        {
            return options.Pipe(x => x.Predicate = predicate);
        }

        public static WalkOptions MaxDepth(this WalkOptions options, int maxDepth)
        {
            return options.Where(node => node.Depth < maxDepth);
        }

        public static WalkOptions OnException(this WalkOptions options, Action<IDirectoryTreeNode, Exception> onException)
        {
            return options.Pipe(node => node.Catch = onException);
        }
    }
}