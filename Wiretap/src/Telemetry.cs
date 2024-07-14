using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Wiretap.Reusable;

namespace Reusable.Wiretap;

public class Telemetry(ILogger logger)
{
    public virtual Type? Type => default;

    public PropertyFactory Properties { get; set; } =
    [
        new ExecutionProperty.Provider(),
        new ProcedureProperty.Provider(),
        new TraceProperty.Provider(),
        new ExceptionProperty.Provider(),
        new SourceProperty.Provider(),
    ];

    public void Log(Procedure procedure, Trace trace, Exception? exception)
    {
        logger.Log(Properties.Create(new Item(procedure, trace, exception, Type)));
    }

    public record Item(Procedure Procedure, Trace Trace, Exception? Exception, Type? Type);
}

public class Telemetry<T>(ILogger logger) : Telemetry(logger)
{
    public override Type? Type => typeof(T);
}

public static class TelemetryExtensions
{
    [MustUseReturnValue]
    public static Procedure LogBegin
    (
        this Telemetry telemetry,
        string? name = default,
        string? message = default,
        object? data = default,
        IEnumerable<string>? tags = default,
        [CallerMemberName] string func = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0
    )
    {
        return new Procedure(telemetry)
        {
            Name = name ?? func,
            Data = data,
            Tags = tags ?? [],
            Source = new(telemetry.Type, func, file, line)
        }.Also
        (
            activity => activity.Log
            (
                new Trace
                (
                    Name: "begin",
                    Message: message,
                    Data: default,
                    Tags: []
                ),
                inProgress: true
            )
        );
    }
}