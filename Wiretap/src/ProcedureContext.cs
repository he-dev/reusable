using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Extensions;

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
    IEnumerable<object>? Tags,
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

public record Source(string Func, string File, int Line);

public interface IProcedure : IDisposable, IEnumerable<IProcedure>
{
    public Guid Id { get; }
    public string Name { get; }
    public Source Source { get; }
    public TimeSpan Elapsed { get; }

    IProcedure Log
    (
        Trace trace,
        Exception? exception = default,
        bool inProgress = true
    );
}

public class ProcedureContext
(
    ILogger logger,
    string name,
    Source source
) : IProcedure
{
    public bool InProgress { get; private set; } = true;

    private Action? Pop { get; set; }
    private Stopwatch Stopwatch { get; } = Stopwatch.StartNew();

    public Guid Id { get; } = Guid.NewGuid();
    public string Name { get; } = name;
    public Source Source { get; } = source;
    public TimeSpan Elapsed => Stopwatch.Elapsed;
    public int TraceCount { get; private set; }

    public IProcedure Log
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

        TraceCount++;
        InProgress = inProgress;
        logger.Log(Properties());
        return this;

        IEnumerable<LogProperty> Properties()
        {
            var first = this.Select(x => x).First();
            yield return new LogProperty("execution", new
            {
                first.Id,
                Path = this.Select(x => x.Name).ToList(),
                first.Elapsed.TotalSeconds
            });

            yield return new LogProperty("procedure", new
            {
                Id,
                Name,
                Elapsed = Elapsed.TotalSeconds,
                Depth = this.Count()
            });

            yield return new LogProperty("trace", new
            {
                trace.Name,
                trace.Message,
                trace.Data,
                Tags = (trace.Tags ?? []).Select(x => x.ToString()).ToHashSet(new StringComparerLite())
            });

            if (exception is not null)
            {
                yield return new LogProperty("exception", exception);
            }

            if (TraceCount == 1)
            {
                yield return new LogProperty("source", Source);
            }
        }
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
    public IEnumerator<IProcedure> GetEnumerator() => AsyncScope<ProcedureContext>.Enumerate().GetEnumerator();

    [MustDisposeResource]
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}