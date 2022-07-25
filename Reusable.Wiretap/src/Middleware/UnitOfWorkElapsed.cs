using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Middleware;

/// <summary>
/// This node adds 'Elapsed' time to the log.
/// </summary>
public class UnitOfWorkElapsed : LoggerMiddleware
{
    public IStopwatch Stopwatch { get; set; } = DefaultStopwatch.StartNew();
    
    public override void Invoke(ILogEntry entry)
    {
        entry.Elapsed(Stopwatch.Elapsed);
        Next?.Invoke(entry);
    }
}