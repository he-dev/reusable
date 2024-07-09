using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.Wiretap;

public static class LoggerExtensions
{
    [MustUseReturnValue]
    public static IProcedure LogBegin
    (
        this ILogger logger,
        string? name = default,
        string? message = default,
        object? data = default,
        IEnumerable<object>? tags = default,
        [CallerMemberName] string func = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0
    )
    {
        return new ProcedureContext(logger, name ?? func, new Source(func, file, line)).Also
        (
            activity => activity.Log
            (
                new Trace
                (
                    Name: "begin",
                    Message: message,
                    Data: data,
                    Tags: tags
                ),
                inProgress: true
            )
        );
    }
}