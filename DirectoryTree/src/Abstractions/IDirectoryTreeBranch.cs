namespace Reusable.IO.Abstractions;

public interface IDirectoryTreeBranch
{
    string Root { get; }

    string Path { get; }

    int Depth { get; }
}