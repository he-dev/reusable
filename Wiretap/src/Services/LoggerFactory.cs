using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Services;

public class LoggerFactory
{
    public LoggerFactory(LogAction log)
    {
        Log = log;
    }

    private LogAction Log { get; }

    public ILogger CreateLogger<T>() => new Logger { Owner = typeof(T), Log = Log };
}