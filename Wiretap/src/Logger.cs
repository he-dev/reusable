using System;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Services;

namespace Reusable.Wiretap;

public class Logger : ILogger
{
    public required Type Owner { get; init; }

    public required LogAction Log { get; init; }
}

public class Logger<T> : ILogger<T>
{
    public Logger(LoggerFactory factory)
    {
        Inner = factory.CreateLogger<T>();
    }

    private ILogger Inner { get; }

    Type ILogger.Owner => Inner.Owner;

    LogAction ILogger.Log => Inner.Log;
}