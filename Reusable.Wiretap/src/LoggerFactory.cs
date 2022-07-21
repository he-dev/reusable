using System.Collections.Concurrent;
using Reusable.Essentials;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap;

public class LoggerFactory : ILoggerFactory
{
    private ConcurrentDictionary<SoftString, ILogger> Loggers { get; } = new();

    public LoggerFactory(ILoggerPipelineBuilder pipelineBuilder) => PipelineBuilder = pipelineBuilder;

    private ILoggerPipelineBuilder PipelineBuilder { get; }

    #region ILoggerFactory

    public ILogger CreateLogger(string name) => Loggers.GetOrAdd(name!, new Logger(PipelineBuilder.Build(name)));

    public void Dispose()
    {
        foreach (var (_, logger) in Loggers)
        {
            logger.Dispose();
        }

        Loggers.Clear();
    }

    #endregion
}