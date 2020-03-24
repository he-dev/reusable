using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
using Reusable.Collections.Generic;
using Reusable.OmniLog.Extensions;

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// This node turn logger-scope on or off. By default it logs BeginScope and EndScope entries for each scope.
    /// </summary>
    public class ToggleScope : LoggerNode
    {
        public override bool Enabled => AsyncScope<ILoggerScope>.Any;

        public Func<IEnumerable<ILoggerNode>> CreateNodes { get; set; } = Enumerable.Empty<ILoggerNode>;

        public ILoggerScope Current => AsyncScope<ILoggerScope>.Current?.Value ?? throw new InvalidOperationException($"Cannot use {nameof(Current)} when {nameof(ToggleScope)} is disabled. Use Logger.BeginScope() first.");

        public Action<ILogger, string> OnBeginScope { get; set; } = (logger, name) => logger.Log(Execution.Context.BeginScope(name));

        public Action<ILogger, Exception?> OnEndScope { get; set; } = (logger, exception) => logger.Log(Execution.Context.EndScope(exception));

        public ILoggerScope Push(ILogger logger, string name)
        {
            try
            {
                var scope = new LoggerScope
                {
                    Logger = logger,
                    First = CreatePipeline(this, CreateNodes()),
                    OnEndScope = OnEndScope
                };
                return AsyncScope<ILoggerScope>.Push(scope).Value;
            }
            finally
            {
                OnBeginScope(Next.First().Node<Logger>(), name);
            }
        }

        public override void Invoke(ILogEntry request)
        {
            // Does not call InvokeNext because it routes the request over the scope which is connected to the next node.
            Current.First.Invoke(request);
        }

        private static ILoggerNode CreatePipeline(ILoggerNode main, IEnumerable<ILoggerNode> branch)
        {
            var last = branch.Join();
            last.Next = main.Next;
            var first = last.First();
            first.Prev = main;

            return first;
        }
    }

    public static class ToggleScopeHelper
    {
        /// <summary>
        /// Creates a new scope that is open until disposed.
        /// </summary>
        public static ILoggerScope BeginScope(this ILogger logger, string name)
        {
            return logger.Node<ToggleScope>().Push(logger, name);
        }

        /// <summary>
        /// Gets the current scope.
        /// </summary>
        public static ILoggerScope Scope(this ILogger logger)
        {
            return logger.Node<ToggleScope>().Current;
        }
    }
}