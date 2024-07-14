using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Wiretap.Reusable;

namespace Reusable.Wiretap;

public enum Severity
{
    High,
    Normal,
    Low
}

public record Trace
(
    string Name,
    string? Message,
    object? Data,
    IEnumerable<string> Tags,
    Severity Severity = Severity.Normal
)
{
    public static T GetNameByCaller<T>
    (
        Func<string, T> trace,
        [CallerMemberName] string name = ""
    )
    {
        return trace(name.RegexReplace("^Log").ToLower());
    }
}

public record Source(Type? Type, string Func, string File, int Line);

public class Procedure(Telemetry telemetry) : IDisposable, IEnumerable<Procedure>
{
    private Action? Pop { get; set; }
    private Stopwatch Stopwatch { get; } = Stopwatch.StartNew();

    public Guid Id { get; } = Guid.NewGuid();

    public required string Name { get; init; }
    public required object? Data { get; init; }
    public required IEnumerable<string> Tags { get; init; }
    public required Source Source { get; init; }

    public TimeSpan Elapsed => Stopwatch.Elapsed;
    public int Traces { get; private set; }
    public bool InProgress { get; private set; } = true;

    public Procedure Log
    (
        Trace trace,
        Exception? exception = default,
        bool inProgress = true
    )
    {
        Pop ??= AsyncScope.Push(this);

        if (!InProgress)
        {
            if (inProgress)
            {
                throw new InvalidOperationException($"Cannot log trace '{trace.Name}' of procedure '{Name}' as it is no longer in progress.");
            }

            return this;
        }

        Traces++;
        InProgress = inProgress;
        telemetry.Log(this, trace, exception);
        return this;
    }

    public void Dispose()
    {
        // End the procedure if the user hasn't done that yet.
        if (InProgress)
        {
            this.LogEnd();
        }

        Pop?.Invoke();
    }

    [MustDisposeResource]
    public IEnumerator<Procedure> GetEnumerator() => AsyncScope<Procedure>.Enumerate().GetEnumerator();

    [MustDisposeResource]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}