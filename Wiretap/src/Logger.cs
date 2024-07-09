using System;
using System.Collections.Generic;
using System.Linq;

namespace Reusable.Wiretap;

public record LogProperty(string Name, object? Value);

public interface ILogger
{
    void Log(IEnumerable<LogProperty> properties);
}

public class Logger(IEnumerable<ILogger> loggers) : ILogger
{
    public virtual void Log(IEnumerable<LogProperty> properties)
    {
        properties = properties.ToList();
        foreach (var logger in loggers)
        {
            logger.Log(properties);
        }
    }
}

public interface ILogger<T> : ILogger { }

public class Logger<T>(IEnumerable<ILogger> loggers) : Logger(loggers), ILogger<T>
{
    public override void Log(IEnumerable<LogProperty> properties)
    {
        base.Log(properties.Append(new LogProperty(nameof(Type), typeof(T))));
    }
}