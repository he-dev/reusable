using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Reusable.Extensions;
using Reusable.IO.Abstractions;
using SysPath = System.IO.Path;

namespace Reusable.IO;

public class DirectoryTree : IDirectoryTree
{
    public IEnumerable<IDirectoryTreeBranch> Walk(string path)
    {
        var branches = new Queue<IDirectoryTreeBranch> { new Branch(path, path) };

        foreach (var branch in branches.Consume())
        {
            if (branch.Path == path)
            {
                yield return branch;
            }

            foreach (var directory in branch.Directories())
            {
                yield return new Branch(path, directory.Path).Also(branches.Enqueue);
            }
        }
    }
    
    public record Branch(string Root, string Path) : IDirectoryTreeBranch
    {
        public int Depth => Path.Split(SysPath.DirectorySeparatorChar).Length - Path.Split(Root).Length;
    }

    public record Item(string Root, string Path, string Name)
    {
        //public string Relative => Regex.Replace(Path, $"^{Root}", string.Empty);
        public string Relative
        {
            get
            {
                var rootLength = Root.Split(SysPath.DirectorySeparatorChar).Length;
                var relative = Path.Split(SysPath.DirectorySeparatorChar).Skip(rootLength).ToArray();
                return SysPath.Combine(relative);
            }
        }

        public override string ToString() => Path;
    }
}