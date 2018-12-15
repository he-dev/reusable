using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;

// todo - do not split yet - it's still being reviewed

namespace Reusable.Stratus
{
    public interface IDirectoryTree
    {
        [NotNull, ItemNotNull]
        IEnumerable<IDirectoryTreeNode> Walk([NotNull] string path, Func<IDirectoryTreeNode, bool> predicate, [NotNull] Action<Exception> onException);
    }

    public class DirectoryTree : IDirectoryTree
    {
        public static Action<Exception> IgnoreExceptions { get; } = _ => { };

        public static Func<IDirectoryTreeNode, bool> Unfiltered { get; } = _ => true;

        /// <summary>
        /// Specifies the max depth of the directory tree. The upper limit is exclusive.
        /// </summary>
        public static Func<IDirectoryTreeNode, bool> MaxDepth(int maxDepth) => node => node.Depth < maxDepth;

        public IEnumerable<IDirectoryTreeNode> Walk(string path, Func<IDirectoryTreeNode, bool> predicate, Action<Exception> onException)
        {
            if (path == null) throw new ArgumentNullException(nameof(path));
            if (onException == null) throw new ArgumentNullException(nameof(onException));

            var nodes = new Queue<DirectoryTreeNode>
            {
                new DirectoryTreeNode(path)
            };

            while (nodes.Any() && nodes.Dequeue() is var current && predicate(current))
            {
                yield return current;

                try
                {
                    foreach (var directory in current.DirectoryNames)
                    {
                        nodes.Enqueue(new DirectoryTreeNode(Path.Combine(current.DirectoryName, directory), current.Depth + 1));
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

        int Depth { get; }

        [NotNull, ItemNotNull]
        IEnumerable<string> DirectoryNames { get; }

        [NotNull, ItemNotNull]
        IEnumerable<string> FileNames { get; }
    }

    internal class DirectoryTreeNode : IDirectoryTreeNode
    {
        internal DirectoryTreeNode(string path, int depth = 0)
        {
            DirectoryName = path;
            Depth = depth;
        }

        public string DirectoryName { get; }

        public int Depth { get; }

        public IEnumerable<string> DirectoryNames => Directory.EnumerateDirectories(DirectoryName).Select(Path.GetFileName);

        public IEnumerable<string> FileNames => Directory.EnumerateFiles(DirectoryName).Select(Path.GetFileName);
    }

    public class RelativeDirectoryTree : IDirectoryTree
    {
        private readonly IDirectoryTree _directoryTree;

        private readonly string _basePath;

        public RelativeDirectoryTree([NotNull] IDirectoryTree directoryTree, [NotNull] string basePath)
        {
            _directoryTree = directoryTree ?? throw new ArgumentNullException(nameof(directoryTree));
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        }

        public IEnumerable<IDirectoryTreeNode> Walk(string path, Func<IDirectoryTreeNode, bool> predicate, Action<Exception> onException)
        {
            return _directoryTree.Walk(Path.Combine(_basePath, path), predicate, onException);
        }
    }

    internal class DirectoryTreeNodeFilter : IDirectoryTreeNode
    {
        internal DirectoryTreeNodeFilter(string path, int depth, IEnumerable<string> directoryNames, IEnumerable<string> fileNames)
        {
            DirectoryName = path;
            Depth = depth;
            DirectoryNames = directoryNames;
            FileNames = fileNames;
        }

        public int Depth { get; }

        public string DirectoryName { get; }

        public IEnumerable<string> DirectoryNames { get; }

        public IEnumerable<string> FileNames { get; }
    }

    public static class DirectoryTreeExtensions
    {
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
                    node.Depth,
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
                    node.Depth,
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
                    node.Depth,
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
                    node.Depth,
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
