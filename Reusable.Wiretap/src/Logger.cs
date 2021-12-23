using Reusable.Essentials.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Data;

namespace Reusable.Wiretap;

public class Logger : LoggerNode, ILogger
{
    public string Name { get; init; } = "Unknown";

    public override void Invoke(ILogEntry entry) => InvokeNext(entry.Push(new LoggableProperty.Logger(Name)));

    public virtual void Log(ILogEntry logEntry) => Invoke(logEntry);
}

// This decorator supports DI.
public class Logger<T> : Logger, ILogger<T>
{
    private readonly ILogger _logger;

    // This constructor makes it easier to create a typed logger with DI.
    public Logger(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger(typeof(T).ToPrettyString());
    }

    public static ILogger<T> Empty { get; } = new EmptyLogger();

    public override ILoggerNode? Prev
    {
        get => _logger.Prev;
        set => _logger.Prev = value;
    }

    public override ILoggerNode? Next
    {
        get => _logger.Next;
        set => _logger.Next = value;
    }

    public override void Log(ILogEntry logEntry) => _logger.Log(logEntry);

    private class EmptyLogger : Logger, ILogger<T> { }
}