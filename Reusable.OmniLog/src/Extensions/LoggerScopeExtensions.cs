using System;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.Extensions
{
    public static class LoggerScopeExtensions
    {
        public static ILoggerScope WithCorrelationHandle(this ILoggerScope logger, object? correlationHandle)
        {
            return logger.Pipe(l => l.Scope().First.Node<Correlate>().CorrelationHandle = correlationHandle);
        }

        public static ILoggerScope UseBuffer(this ILoggerScope logger)
        {
            // Branch-node is properly initialized at this point.
            return logger.Pipe(x => x.Node<Branch>().First.Node<BufferLog>().Enable());
        }

        /// <summary>
        /// Activates a new MemoryNode.
        /// </summary>
        public static ILoggerScope UseInMemoryCache(this ILoggerScope logger)
        {
            return logger.Pipe(x => x.Node<Branch>().First.Node<CacheInMemory>().Enable());
        }

        /// <summary>
        /// Activates a new stopwatch and returns it.
        /// </summary>
        [Obsolete("Don't use this API. It's been replaced by the BranchNode. It's empty and provided for backward compatibility.")]
        public static ILoggerScope UseStopwatch(this ILoggerScope scope)
        {
            //return scope.Append(new StopwatchNode());
            return scope;
        }
    }
}