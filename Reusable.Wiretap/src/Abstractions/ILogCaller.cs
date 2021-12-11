namespace Reusable.Wiretap.Abstractions;

public interface ILogCaller
{
    string MemberName { get; }

    int LineNumber { get; }

    string FilePath { get; }
}