using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reusable.Extensions;

namespace Reusable.IO.DirectoryTree
{
    public class PhysicalDirectoryTree : IDirectoryTree
    {
        public Func<string, string, string> Combine { get; } = Path.Combine;

        public IEnumerable<IDirectoryTreeNode> Walk(string path, WalkOptions? options = default)
        {
            options ??= WalkOptions.None;

            var nodes = new Queue<IDirectoryTreeNode>
            {
                new PhysicalDirectoryTreeNode(path)
            };

            foreach (var current in nodes.Consume().Where(options.Predicate))
            {
                yield return current;

                try
                {
                    foreach (var directory in current.DirectoryNames)
                    {
                        nodes.Enqueue(new PhysicalDirectoryTreeNode(Combine(current.DirectoryName, directory), current.Depth + 1));
                    }
                }
                catch (Exception inner)
                {
                    if (options.Catch is {} onException)
                    {
                        onException(current, inner);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }
    }
}