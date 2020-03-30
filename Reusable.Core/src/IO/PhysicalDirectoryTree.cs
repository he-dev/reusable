using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.IO
{
    [PublicAPI]
    public class WalkOptions
    {
        public Func<IDirectoryTreeNode, bool> Predicate { get; set; } = _ => true;

        public Action<Exception> OnException { get; set; } = ex => throw ex;

        public static WalkOptions None => new WalkOptions();
    }

    [PublicAPI]
    public static class WalkOptionsExtensions
    {
        public static WalkOptions SuppressExceptions(this WalkOptions options)
        {
            return options.Pipe(x => x.OnException = _ => { });
        }

        public static WalkOptions Where(this WalkOptions options, Func<IDirectoryTreeNode, bool> predicate)
        {
            return options.Pipe(x => x.Predicate = predicate);
        }
        
        public static WalkOptions MaxDepth(this WalkOptions options, int maxDepth)
        {
            return options.Where(node => node.Depth < maxDepth);
        }
    }

    public interface IDirectoryTree
    {
        IEnumerable<IDirectoryTreeNode> Walk(string path, WalkOptions? options = default);

        Func<string, string, string> Combine { get; }
    }

    public class PhysicalDirectoryTree : IDirectoryTree
    {
        public Func<string, string, string> Combine { get; } = Path.Combine;

        public IEnumerable<IDirectoryTreeNode> Walk(string path, WalkOptions? options = default)
        {
            options ??= new WalkOptions();

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
                    options.OnException(inner);
                }
            }
        }
    }

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

    [PublicAPI]
    public interface IDirectoryTreeNode
    {
        string DirectoryName { get; }

        int Depth { get; }

        IEnumerable<string> DirectoryNames { get; }

        IEnumerable<string> FileNames { get; }

        Func<string, string, string> Combine { get; }
    }

    public class DirectoryTreeNode : IDirectoryTreeNode
    {
        public DirectoryTreeNode(string path, int depth, IEnumerable<string> directoryNames, IEnumerable<string> fileNames, Func<string, string, string> combine)
        {
            DirectoryName = path;
            Depth = depth;
            DirectoryNames = directoryNames;
            FileNames = fileNames;
            Combine = combine;
        }

        public int Depth { get; }

        public string DirectoryName { get; }

        public IEnumerable<string> DirectoryNames { get; }

        public IEnumerable<string> FileNames { get; }

        public Func<string, string, string> Combine { get; }

        public override string ToString()
        {
            return ToStringFields().Join(", ");
        }

        private IEnumerable<string> ToStringFields()
        {
            yield return $"'{DirectoryName}'";
            yield return $"Depth: {Depth}";
#if DEBUG
            yield return $"Directories: {DirectoryNames.Count()}";
            yield return $"Files: {FileNames.Count()}";
#endif
        }
    }

    internal class PhysicalDirectoryTreeNode : DirectoryTreeNode
    {
        internal PhysicalDirectoryTreeNode(string directoryName, int depth = 0) : base
        (
            directoryName,
            depth,
            Directory.EnumerateDirectories(directoryName).Select(Path.GetFileName),
            Directory.EnumerateFiles(directoryName).Select(Path.GetFileName),
            Path.Combine
        ) { }
    }

    public static class DirectoryTreeExtensions
    {
        public static IEnumerable<IDirectoryTreeNode> Walk(this IDirectoryTree tree, string path, Func<IDirectoryTreeNode, bool>? predicate = default, Action<Exception>? onException = default)
        {
            return tree.Walk(path, new WalkOptions
            {
                Predicate = predicate ?? (_ => true),
                OnException = onException ?? (_ => { })
            });
        }

        public static IEnumerable<IDirectoryTreeNode> WhereDirectory(this IEnumerable<IDirectoryTreeNode> nodes, Func<string, bool> predicate)
        {
            return
                from node in nodes
                select new DirectoryTreeNode
                (
                    node.DirectoryName,
                    node.Depth,
                    from directoryName in node.DirectoryNames where predicate(node.Combine(node.DirectoryName, directoryName)) select directoryName,
                    node.FileNames,
                    node.Combine
                );
        }

        public static IEnumerable<IDirectoryTreeNode> WhereFile(this IEnumerable<IDirectoryTreeNode> nodes, Func<string, bool> predicate)
        {
            return
                from node in nodes
                select new DirectoryTreeNode
                (
                    node.DirectoryName,
                    node.Depth,
                    node.DirectoryNames,
                    from fileName in node.FileNames where predicate(node.Combine(node.DirectoryName, fileName)) select fileName,
                    node.Combine
                );
        }
    }

    public static class DirectoryTreeNodeExtensions
    {
        /// <summary>
        /// Enumerates full directory names.
        /// </summary>
        public static IEnumerable<string> Directories(this IEnumerable<IDirectoryTreeNode> nodes)
        {
            return
                from node in nodes
                from name in node.DirectoryNames
                select node.Combine(node.DirectoryName, name);
        }

        /// <summary>
        /// Enumerates full file names.
        /// </summary>
        public static IEnumerable<string> Files(this IEnumerable<IDirectoryTreeNode> nodes)
        {
            return
                from node in nodes
                from name in node.FileNames
                select node.Combine(node.DirectoryName, name);
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