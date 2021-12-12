using System;
using System.Collections.Generic;
using Reusable.Extensions;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Nodes;
using Reusable.Wiretap.Services.Properties;

namespace Reusable.Wiretap.Extensions;

public static class LoggerFactoryExtensions
{
    public static ILogger<T> CreateLogger<T>(this ILoggerFactory loggerFactory) => new Logger<T>(loggerFactory);

    public static LoggerFactory Configure<T>(this LoggerFactory loggerFactory, Action<T> configure) where T : ILoggerNode
    {
        loggerFactory.PipelineConfiguration = nodes => nodes.AddConfiguration(configure);
        return loggerFactory;
    }

    private static IEnumerable<ILoggerNode> AddConfiguration<T>(this IEnumerable<ILoggerNode> nodes, Action<T> configure) where T : ILoggerNode
    {
        foreach (var node in nodes)
        {
            if (node is T configurable)
            {
                configure(configurable);
            }

            yield return node;
        }
    }

    public static LoggerFactory Environment(this LoggerFactory loggerFactory, string name)
    {
        return loggerFactory.Configure<InvokePropertyService>(node =>
        {
            node.Services.Add(new Constant(nameof(Environment), name));
        });
    }
    
    public static LoggerFactory Product(this LoggerFactory loggerFactory, string name)
    {
        return loggerFactory.Configure<InvokePropertyService>(node =>
        {
            node.Services.Add(new Constant(nameof(Product), name));
        });
    }
    
    public static LoggerFactory Echo(this LoggerFactory loggerFactory, IConnector connector)
    {
        return loggerFactory.Configure<Echo>(node =>
        {
            node.Connectors.Add(connector);
        });
    }
    
    public static LoggerFactory Echo<T>(this LoggerFactory loggerFactory, Action<T>? configure = default) where T : IConnector, new()
    {
        return loggerFactory.Configure<Echo>(node =>
        {
            node.Connectors.Add(new T().Also(configure));
        });
    }
}