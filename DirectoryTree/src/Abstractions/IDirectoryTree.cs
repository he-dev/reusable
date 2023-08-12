using System;
using System.Collections.Generic;

namespace Reusable.IO.Abstractions;

public interface IDirectoryTree
{
    IEnumerable<IDirectoryTreeBranch> Walk(string path);
}