using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.IO
{
    public interface IDirectoryTree
    {
        [NotNull, ItemNotNull]
        IEnumerable<IDirectoryTreeNode> Walk([NotNull] string path, [NotNull] Action<Exception> onException);
    }

    public class DirectoryTree : IDirectoryTree
    {
        public IEnumerable<IDirectoryTreeNode> Walk(string path, Action<Exception> onException)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (onException == null) throw new ArgumentNullException(nameof(onException));

            var nodes = new Queue<DirectoryTreeNode>
            {
                new DirectoryTreeNode(path)
            };

            while (nodes.Any())
            {
                var current = nodes.Dequeue();
                yield return current;

                try
                {
                    foreach (var directory in current.DirectoryNames)
                    {
                        nodes.Enqueue(new DirectoryTreeNode(Path.Combine(current.DirectoryName, directory)));
                    }
                }
                catch (Exception inner)
                {
                    onException(inner);
                }
            }
        }
    }

    [PublicAPI]
    public interface IDirectoryTreeNode
    {
        [NotNull]
        string DirectoryName { get; }

        [NotNull, ItemNotNull]
        IEnumerable<string> DirectoryNames { get; }

        [NotNull, ItemNotNull]
        IEnumerable<string> FileNames { get; }
    }

    internal class DirectoryTreeNode : IDirectoryTreeNode
    {
        internal DirectoryTreeNode(string path)
        {
            DirectoryName = path;
        }

        public string DirectoryName { get; }

        public IEnumerable<string> DirectoryNames => Directory.EnumerateDirectories(DirectoryName).Select(Path.GetFileName);

        public IEnumerable<string> FileNames => Directory.EnumerateFiles(DirectoryName).Select(Path.GetFileName);
    }

    internal class DirectoryTreeNodeFilter : IDirectoryTreeNode
    {
        internal DirectoryTreeNodeFilter(string path, IEnumerable<string> directoryNames, IEnumerable<string> fileNames)
        {
            DirectoryName = path;
            DirectoryNames = directoryNames;
            FileNames = fileNames;
        }

        public string DirectoryName { get; }

        public IEnumerable<string> DirectoryNames { get; }

        public IEnumerable<string> FileNames { get; }
    }

    public static class DirectoryTreeExtensions
    {
        [NotNull, ItemNotNull]
        public static IEnumerable<IDirectoryTreeNode> WalkSilently([NotNull] this IDirectoryTree directoryTree, [NotNull] string path)
        {
            if (directoryTree == null) throw new ArgumentNullException(nameof(directoryTree));
            if (path == null) throw new ArgumentNullException(nameof(path));

            return directoryTree.Walk(path, _ => { });
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<IDirectoryTreeNode> SkipDirectories([NotNull] this IEnumerable<IDirectoryTreeNode> nodes, [NotNull][RegexPattern] string directoryNamePattern)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (directoryNamePattern == null) throw new ArgumentNullException(nameof(directoryNamePattern));

            return
                from node in nodes
                where !node.DirectoryName.Matches(directoryNamePattern)
                select new DirectoryTreeNodeFilter
                (
                    node.DirectoryName,
                    from dirname in node.DirectoryNames where !dirname.Matches(directoryNamePattern) select dirname,
                    node.FileNames
                );
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<IDirectoryTreeNode> SkipFiles([NotNull] this IEnumerable<IDirectoryTreeNode> nodes, [NotNull][RegexPattern] string fileNamePattern)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (fileNamePattern == null) throw new ArgumentNullException(nameof(fileNamePattern));

            return
                from node in nodes
                select new DirectoryTreeNodeFilter
                (
                    node.DirectoryName,
                    node.DirectoryNames,
                    from fileName in node.FileNames where !fileName.Matches(fileNamePattern) select fileName
                );
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<IDirectoryTreeNode> WhereDirectories([NotNull] this IEnumerable<IDirectoryTreeNode> nodes, [NotNull][RegexPattern] string directoryNamePattern)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (directoryNamePattern == null) throw new ArgumentNullException(nameof(directoryNamePattern));

            return
                from node in nodes
                where node.DirectoryName.Matches(directoryNamePattern)
                select new DirectoryTreeNodeFilter
                (
                    node.DirectoryName,
                    from dirname in node.DirectoryNames where dirname.Matches(directoryNamePattern) select dirname,
                    node.FileNames
                );
        }

        [NotNull, ItemNotNull]
        public static IEnumerable<IDirectoryTreeNode> WhereFiles([NotNull] this IEnumerable<IDirectoryTreeNode> nodes, [NotNull][RegexPattern] string fileNamePattern)
        {
            if (nodes == null) throw new ArgumentNullException(nameof(nodes));
            if (fileNamePattern == null) throw new ArgumentNullException(nameof(fileNamePattern));

            return
                from node in nodes
                select new DirectoryTreeNodeFilter
                (
                    node.DirectoryName,
                    node.DirectoryNames,
                    from fileName in node.FileNames
                    where fileName.Matches(fileNamePattern)
                    select fileName
                );
        }

        private static bool Matches(this string name, [RegexPattern] string pattern)
        {
            return Regex.IsMatch(name, pattern, RegexOptions.IgnoreCase);
        }
    }

    public static class DirectoryTreeNodeExtensions
    {
        public static void Deconstruct(
            [CanBeNull] this IDirectoryTreeNode directoryTreeNode,
            [CanBeNull] out string directoryName,
            [CanBeNull] out IEnumerable<string> directoryNames,
            [CanBeNull] out IEnumerable<string> fileNames)
        {
            directoryName = directoryTreeNode?.DirectoryName;
            directoryNames = directoryTreeNode?.DirectoryNames;
            fileNames = directoryTreeNode?.FileNames;
        }

        public static bool Exists(
            [CanBeNull] this IDirectoryTreeNode directoryTreeNode)
        {
            // Empty string does not exist and it'll return false.
            return Directory.Exists(directoryTreeNode?.DirectoryName ?? string.Empty);
        }
    }
}
