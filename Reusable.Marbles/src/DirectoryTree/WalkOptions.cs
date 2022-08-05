using System;
using JetBrains.Annotations;

namespace Reusable.Marbles.DirectoryTree;

[PublicAPI]
public class WalkOptions
{
    public Func<IDirectoryTreeNode, bool> Predicate { get; set; } = _ => true;

    public Action<IDirectoryTreeNode, Exception>? Catch { get; set; }

    public static WalkOptions None => new();
}