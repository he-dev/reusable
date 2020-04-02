using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Reusable.IO.DirectoryTree
{
    [PublicAPI]
    public interface IDirectoryTreeNode
    {
        string DirectoryName { get; }

        int Depth { get; }

        IEnumerable<string> DirectoryNames { get; }

        IEnumerable<string> FileNames { get; }

        /// <summary>
        /// Gets the function that combines path segments.
        /// </summary>
        Func<string, string, string> Combine { get; }
    }
}