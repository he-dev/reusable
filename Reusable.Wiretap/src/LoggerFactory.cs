using System.Collections.Concurrent;
using Reusable.Marbles;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap;

public class LoggerFactory : ILoggerFactory
{
    private ConcurrentDictionary<string, ILogger> Loggers { get; } = new(SoftString.Comparer);

    public LoggerFactory(ILoggerBuilder builder) => Builder = builder;

    private ILoggerBuilder Builder { get; }

    #region ILoggerFactory

    public ILogger CreateLogger(string name) => Loggers.GetOrAdd(name, Builder.Build(name));

    #endregion
}