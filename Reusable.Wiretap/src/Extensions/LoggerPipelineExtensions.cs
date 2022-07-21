using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Nodes;

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

    public static IEnumerable<ILoggerNode> Expand(this IEnumerable<ILoggerNode> nodes)
    {
        foreach (var node in nodes)
        {
            if (node is IEnumerable<ILoggerNode> group)
            {
                foreach (var item in group)
                {
                    yield return item;
                }
            }
            else
            {
                yield return node;
            }
        }
    }
}