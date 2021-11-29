using System;
using System.Collections.Generic;
using System.Linq;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap.Extensions
{
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
            return nodes.Where(node => !(node is T));
        }

        // public static ILoggerNodeConfiguration<T> Configure<T>(this IEnumerable<ILoggerNode> nodes) where T : ILoggerNode
        // {
        //     return new LoggerNodeConfiguration<T>(nodes);
        // }

        public static ILoggerFactory ToLoggerFactory(this IEnumerable<ILoggerNode> nodes) => new LoggerFactory(nodes);
    }

    // ReSharper disable once UnusedTypeParameter - This interface carries the T so it must not be removed.
    // public interface ILoggerNodeConfiguration<T> : IEnumerable<ILoggerNode> { }
    //
    // internal class LoggerNodeConfiguration<T> : ILoggerNodeConfiguration<T> where T : ILoggerNode
    // {
    //     private readonly IEnumerable<ILoggerNode> _nodes;
    //
    //     public LoggerNodeConfiguration(IEnumerable<ILoggerNode> nodes) => _nodes = nodes;
    //
    //     public IEnumerator<ILoggerNode> GetEnumerator() => _nodes.GetEnumerator();
    //
    //     IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_nodes).GetEnumerator();
    // }
    //
    // public static class LoggerNodeConfigurationExtensions
    // {
    //     public static ILoggerNodeConfiguration<T> Add<T, TValue>
    //     (
    //         this ILoggerNodeConfiguration<T> configuration,
    //         Func<T, ICollection<TValue>> selector,
    //         params TValue[] values
    //     ) where T : ILoggerNode
    //     {
    //         return new LoggerNodeConfiguration<T>(configuration.Configure<T>(node =>
    //         {
    //             foreach (var value in values)
    //             {
    //                 selector(node).Add(value);
    //             }
    //         }));
    //     }
    //
    //     public static ILoggerNodeConfiguration<T> Add<T, TKey, TValue>
    //     (
    //         this ILoggerNodeConfiguration<T> configuration,
    //         Func<T, IDictionary<TKey, TValue>> selector,
    //         params (TKey Key, TValue Value)[] items
    //     ) where T : ILoggerNode
    //     {
    //         return new LoggerNodeConfiguration<T>(configuration.Configure<T>(node =>
    //         {
    //             foreach (var (key, value) in items)
    //             {
    //                 selector(node).Add(key, value);
    //             }
    //         }));
    //     }
    // }
}