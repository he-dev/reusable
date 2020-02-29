using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.IO
{
    public interface IDirectoryTree
    {
        IEnumerable<IDirectoryTreeNode> Walk(string path, Func<IDirectoryTreeNode, bool> predicate, Action<Exception> onException);
    }

    public class PhysicalDirectoryTree : IDirectoryTree
    {
        public static Action<Exception> IgnoreExceptions { get; } = _ => { };

        public static Func<IDirectoryTreeNode, bool> Unfiltered { get; } = _ => true;

        /// <summary>
        /// Specifies the max depth of the directory tree. The upper limit is exclusive.
        /// </summary>
        public static Func<IDirectoryTreeNode, bool> MaxDepth(int maxDepth) => node => node.Depth < maxDepth;

        public IEnumerable<IDirectoryTreeNode> Walk(string path, Func<IDirectoryTreeNode, bool> predicate, Action<Exception> onException)
        {
            path = Environment.ExpandEnvironmentVariables(path);

            var nodes = new Queue<PhysicalDirectoryTreeNode>
            {
                new PhysicalDirectoryTreeNode(path)
            };

            while (nodes.Any() && nodes.Dequeue() is var current && predicate(current))
            {
                yield return current;

                try
                {
                    foreach (var directory in current.DirectoryNames)
                    {
                        nodes.Enqueue(new PhysicalDirectoryTreeNode(Path.Combine(current.DirectoryName, directory), current.Depth + 1));
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
        string DirectoryName { get; }

        int Depth { get; }

        IEnumerable<string> DirectoryNames { get; }

        IEnumerable<string> FileNames { get; }
    }

    internal class PhysicalDirectoryTreeNode : IDirectoryTreeNode
    {
        internal PhysicalDirectoryTreeNode(string path, int depth = 0)
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

        public RelativeDirectoryTree(IDirectoryTree directoryTree, string basePath)
        {
            _directoryTree = directoryTree ?? throw new ArgumentNullException(nameof(directoryTree));
            _basePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        }

        public IEnumerable<IDirectoryTreeNode> Walk(string path, Func<IDirectoryTreeNode, bool> predicate, Action<Exception> onException)
        {
            return _directoryTree.Walk(Path.Combine(_basePath, path), predicate, onException);
        }
    }

    internal class DirectoryTreeNodeView : IDirectoryTreeNode
    {
        internal DirectoryTreeNodeView(string path, int depth, IEnumerable<string> directoryNames, IEnumerable<string> fileNames)
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
        public static IEnumerable<IDirectoryTreeNode> SkipDirectories(this IEnumerable<IDirectoryTreeNode> nodes, [RegexPattern] string directoryNamePattern)
        {
            return
                from node in nodes
                where !node.DirectoryName.Matches(directoryNamePattern)
                select new DirectoryTreeNodeView
                (
                    node.DirectoryName,
                    node.Depth,
                    from dirname in node.DirectoryNames where !dirname.Matches(directoryNamePattern) select dirname,
                    node.FileNames
                );
        }

        public static IEnumerable<IDirectoryTreeNode> SkipFiles(this IEnumerable<IDirectoryTreeNode> nodes, [RegexPattern] string fileNamePattern)
        {
            return
                from node in nodes
                select new DirectoryTreeNodeView
                (
                    node.DirectoryName,
                    node.Depth,
                    node.DirectoryNames,
                    from fileName in node.FileNames where !fileName.Matches(fileNamePattern) select fileName
                );
        }

        public static IEnumerable<IDirectoryTreeNode> WhereDirectories(this IEnumerable<IDirectoryTreeNode> nodes, [RegexPattern] string directoryNamePattern)
        {
            return
                from node in nodes
                where node.DirectoryName.Matches(directoryNamePattern)
                select new DirectoryTreeNodeView
                (
                    node.DirectoryName,
                    node.Depth,
                    from dirname in node.DirectoryNames where dirname.Matches(directoryNamePattern) select dirname,
                    node.FileNames
                );
        }

        public static IEnumerable<IDirectoryTreeNode> WhereFiles(this IEnumerable<IDirectoryTreeNode> nodes, [RegexPattern] string fileNamePattern)
        {
            return
                from node in nodes
                select new DirectoryTreeNodeView
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
        public static void Deconstruct
        (
            this IDirectoryTreeNode? directoryTreeNode,
            out string? directoryName,
            out IEnumerable<string>? directoryNames,
            out IEnumerable<string>? fileNames
        )
        {
            directoryName = directoryTreeNode?.DirectoryName;
            directoryNames = directoryTreeNode?.DirectoryNames;
            fileNames = directoryTreeNode?.FileNames;
        }

        public static bool Exists(this IDirectoryTreeNode? directoryTreeNode)
        {
            // Empty string does not exist and it'll return false.
            return Directory.Exists(directoryTreeNode?.DirectoryName ?? string.Empty);
        }
    }
}