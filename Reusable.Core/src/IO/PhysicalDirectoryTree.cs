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
        IEnumerable<IDirectoryTreeNode> Walk(string path, Func<IDirectoryTreeNode, bool>? predicate = default, Action<Exception>? onException = default);
    }

    public static class DirectoryTreePredicates
    {
        /// <summary>
        /// Specifies the max depth of the directory tree. The upper limit is exclusive.
        /// </summary>
        public static Func<IDirectoryTreeNode, bool> MaxDepth(int maxDepth) => node => node.Depth < maxDepth;
        
        public static Func<IDirectoryTreeNode, bool> Unfiltered { get; } = _ => true;
    }

    public class PhysicalDirectoryTree : IDirectoryTree
    {
        public static Action<Exception> IgnoreExceptions { get; } = _ => { };



        public IEnumerable<IDirectoryTreeNode> Walk(string path, Func<IDirectoryTreeNode, bool>? predicate = default, Action<Exception>? onException = default)
        {
            predicate ??= DirectoryTreePredicates.Unfiltered;
            onException ??= IgnoreExceptions;

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

    public class RelativeDirectoryTree : IDirectoryTree, IDecorator<IDirectoryTree>
    {
        public RelativeDirectoryTree(IDirectoryTree directoryTree, string basePath)
        {
            Decoratee = directoryTree;
            BasePath = basePath ?? throw new ArgumentNullException(nameof(basePath));
        }

        public IDirectoryTree Decoratee { get; }

        public string BasePath { get; }

        public IEnumerable<IDirectoryTreeNode> Walk(string path, Func<IDirectoryTreeNode, bool>? predicate = default, Action<Exception>? onException = default)
        {
            return Decoratee.Walk(Path.Combine(BasePath, path), predicate, onException);
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
        public static IEnumerable<IDirectoryTreeNode> IgnoreDirectories(this IEnumerable<IDirectoryTreeNode> nodes, [RegexPattern] string directoryNamePattern)
        {
            return
                from node in nodes
                where !node.DirectoryName.Matches(directoryNamePattern, RegexOptions.IgnoreCase)
                select new DirectoryTreeNodeView
                (
                    node.DirectoryName,
                    node.Depth,
                    from dirname in node.DirectoryNames where !dirname.Matches(directoryNamePattern, RegexOptions.IgnoreCase) select dirname,
                    node.FileNames
                );
        }

        public static IEnumerable<IDirectoryTreeNode> IgnoreFiles(this IEnumerable<IDirectoryTreeNode> nodes, [RegexPattern] string fileNamePattern)
        {
            return
                from node in nodes
                select new DirectoryTreeNodeView
                (
                    node.DirectoryName,
                    node.Depth,
                    node.DirectoryNames,
                    from fileName in node.FileNames where !fileName.Matches(fileNamePattern, RegexOptions.IgnoreCase) select fileName
                );
        }

        public static IEnumerable<IDirectoryTreeNode> WhereDirectories(this IEnumerable<IDirectoryTreeNode> nodes, [RegexPattern] string directoryNamePattern)
        {
            return
                from node in nodes
                where node.DirectoryName.Matches(directoryNamePattern, RegexOptions.IgnoreCase)
                select new DirectoryTreeNodeView
                (
                    node.DirectoryName,
                    node.Depth,
                    from dirname in node.DirectoryNames where dirname.Matches(directoryNamePattern, RegexOptions.IgnoreCase) select dirname,
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
                    from fileName in node.FileNames where fileName.Matches(fileNamePattern, RegexOptions.IgnoreCase) select fileName
                );
        }
    }

    public static class DirectoryTreeNodeExtensions
    {
        public static IEnumerable<string> FullNames(this IEnumerable<IDirectoryTreeNode> nodes)
        {
            return
                from n in nodes
                from f in n.FileNames
                select Path.Combine(n.DirectoryName, f);
        }

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