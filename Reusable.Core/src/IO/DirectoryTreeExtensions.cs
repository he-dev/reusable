using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using JetBrains.Annotations;

namespace Reusable.IO
{
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
}
