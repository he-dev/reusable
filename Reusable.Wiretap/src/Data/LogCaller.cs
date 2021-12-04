using System.Runtime.CompilerServices;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Data;

public record LogCaller(
    [CallerMemberName] string? MemberName = null,
    [CallerLineNumber] int LineNumber = 0,
    [CallerFilePath] string? FilePath = null
) : ILogCaller;