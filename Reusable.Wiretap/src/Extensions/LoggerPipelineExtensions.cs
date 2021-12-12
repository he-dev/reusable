using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Extensions;

public static class LoggerPipelineExtensions
{
    public static IEnumerable<ILoggerNode> Configure<T>(this IEnumerable<ILoggerNode> nodes, Action<T> configure) where T : ILoggerNode
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

    public static IEnumerable<ILoggerNode> InjectAfter<T>(this IEnumerable<ILoggerNode> nodes, ILoggerNode inject) where T : ILoggerNode
    {
        foreach (var node in nodes)
        {
            if (node is T)
            {
                yield return inject;
            }

            yield return node;
        }
    }
        
    public static IEnumerable<ILoggerNode> Exclude<T>(this IEnumerable<ILoggerNode> nodes) where T : ILoggerNode
    {
        return nodes.Where(node => node is not T);
    }
}