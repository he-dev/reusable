using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Reusable.Collections.Generic;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Pipelines;

namespace Reusable.Wiretap;

public delegate IEnumerable<ILoggerNode> PipelineConfiguration(IEnumerable<ILoggerNode> nodes);

public class LoggerFactory : ILoggerFactory
{
    private ConcurrentDictionary<SoftString, ILogger> Loggers { get; } = new();

    public LoggerPipeline Pipeline { get; set; } = new EmptyPipeline();

    public PipelineConfiguration PipelineConfiguration { get; set; } = nodes => nodes;

    public static LoggerFactory CreateWith<T>() where T : LoggerPipeline, new() => new() { Pipeline = new T() };

    #region ILoggerFactory

    public ILogger CreateLogger(string name) => Loggers.GetOrAdd(name, CreatePipeline(name));

    private ILogger CreatePipeline(string loggerName)
    {
        var configuredPipeline = PipelineConfiguration(Pipeline);
        // Prepend does not work with .net-framework.
        return (ILogger)new ILoggerNode[] { new Logger { Name = loggerName } }.Concat(configuredPipeline).Join().First();
    }

    public void Dispose()
    {
        foreach (var logger in Loggers)
        {
            foreach (var node in ((ILoggerNode)logger.Value).EnumerateNext())
            {
                node.Dispose();
            }
        }

        Loggers.Clear();
    }

    #endregion
}
