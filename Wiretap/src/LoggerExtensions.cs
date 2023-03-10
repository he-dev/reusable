using System.Collections.Immutable;
using System.IO;
using System.Runtime.CompilerServices;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap;

public static class LoggerExtensions
{
    public static LoggerContext StartWithCaller
    (
        this ILogger logger,
        string name,
        object? details = default,
        object? attachment = default,
        [CallerMemberName] string? callerMemberName = null,
        [CallerLineNumber] int? callerLineNumber = 0,
        [CallerFilePath] string? callerFilePath = null
    )
    {
        return logger.Start
        (
            name,
            details.EnumerateProperties().ToImmutableDictionary(SoftString.Comparer).SetItem("caller", new
            {
                memberName = callerMemberName,
                lineNumber = callerLineNumber,
                filePath = Path.GetFileName(callerFilePath)
            }),
            attachment
        );
    }
}