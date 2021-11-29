namespace Reusable.Wiretap.Abstractions;

public interface ICaller
{
    string MemberName { get; }

    int LineNumber { get; }

    string FilePath { get; }
}