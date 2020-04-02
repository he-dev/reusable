using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Custom;

namespace Reusable.IO.DirectoryTree
{
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
}