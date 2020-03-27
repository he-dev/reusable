using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
using Reusable.Collections.Generic;
using Reusable.OmniLog.Data;
using Reusable.OmniLog.Extensions;
using CallSite = Reusable.OmniLog.Data.CallSite;

// ReSharper disable ExplicitCallerInfoArgument - this is fine because it needs to be overriden

namespace Reusable.OmniLog.Nodes
{
    /// <summary>
    /// This node turn logger-scope on or off. By default it logs BeginScope and EndScope entries for each scope.
    /// </summary>
    public class ToggleScope : LoggerNode
    {
        public override bool Enabled => AsyncScope<ILoggerScope>.Any;

        public Func<IEnumerable<ILoggerNode>> CreateNodes { get; set; } = Enumerable.Empty<ILoggerNode>;

        public ILoggerScope Current
        {
            get
            {
                return
                    AsyncScope<ILoggerScope>.Current?.Value
                    ?? throw new InvalidOperationException($"Cannot use {nameof(Current)} when {nameof(ToggleScope)} is disabled. Use Logger.BeginScope() first.");
            }
        }

        public ILoggerScope Push(ILogger logger, string name, object? workItem, CallSite callSite)
        {
            try
            {
                var scope = new LoggerScope
                {
                    Name = name,
                    Logger = logger,
                    WorkItem = workItem,
                    CallSite = callSite,
                    First = CreatePipeline(this, CreateNodes()),
                };
                return AsyncScope<ILoggerScope>.Push(scope).Value;
            }
            finally
            {
                if (Next?.First() is { } first)
                {
                    //OnBeginScope(first.Node<Logger>(), name, callSite);
                }
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
        [MustUseReturnValue]
        public static ILoggerScope BeginScope
        (
            this ILogger logger,
            string name,
            object? workItem = default,
            [CallerMemberName] string? callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string? callerFilePath = null
        )
        {
            return logger.Node<ToggleScope>().Push(logger, name, workItem, new CallSite(callerMemberName, callerLineNumber, callerFilePath));
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