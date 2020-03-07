using System;
using System.Collections.Generic;
using System.Diagnostics;
using Reusable.Extensions;
using Reusable.Flowingo.Abstractions;

namespace Reusable.Flowingo.Services
{
    using static Logger;

    public interface ILoggerContext 
    {
        ILogger Logger { get; }
    }
    
    public interface ILogger
    {
        void Log(WorkflowEvent<> workflowEvent);
    }

    public static class LoggerExtensions
    {
        [DebuggerStepThrough]
        public static void LogBegin<TContext>(this ILogger logger, IStep<TContext> step)
        {
            logger.Log(new WorkflowEvent<TContext>
            {
                Step = step,
                Message = $"{step.GetType().ToPrettyString()} #{step.Tag ?? "?"}",
                Progress = WorkflowProgress.Begin
            });
        }

        [DebuggerStepThrough]
        public static void LogInfo<TContext>(this ILogger logger, IStep<TContext> step, string message)
        {
            logger.Log(new WorkflowEvent<TContext>
            {
                Step = step,
                Message = message,
                Progress = WorkflowProgress.Info
            });
        }

        [DebuggerStepThrough]
        public static void LogBreak<TContext>(this ILogger logger, IStep<TContext> step, string message)
        {
            logger.Log(new WorkflowEvent<TContext>
            {
                Step = step,
                Message = message,
                Progress = WorkflowProgress.Break
            });
        }

        [DebuggerStepThrough]
        public static void LogEnd<TContext>(this ILogger logger, IStep<TContext> step, string message)
        {
            logger.Log(new WorkflowEvent<TContext>
            {
                Step = step,
                Message = message,
                Progress = WorkflowProgress.End
            });
        }
    }

    public class MemoryLogger<TContext> : List<WorkflowEvent<TContext>>, ILogger<TContext>
    {
        [DebuggerStepThrough]
        public void Log(WorkflowEvent<TContext> workflowEvent) => Add(workflowEvent);
    }

    public static class Logger
    {
        public static class Formatting
        {
            [DebuggerStepThrough]
            public static string Timestamp() => DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss:fff");

            [DebuggerStepThrough]
            public static string Tag(string value) => $"[#{value?.Trim('#')}]";
        }
    }

    public class WorkflowEvent<TContext>
    {
        public IStep<TContext> Step { get; set; }

        public WorkflowProgress Progress { get; set; }

        public string Message { get; set; }

        public override string ToString()
        {
            return $"{Formatting.Timestamp()} | {Step.GetType().ToPrettyString()} | {Progress.ToString().ToUpper()} | {Message}";
        }
    }

    public enum WorkflowProgress
    {
        /// <summary>
        /// The workflow proceeded to the next step.
        /// </summary>
        Begin,

        /// <summary>
        /// General info about the execution.
        /// </summary>
        Info,

        /// <summary>
        /// Cannot proceed with the next step.
        /// </summary>
        Break,

        /// <summary>
        /// Proceed to the next step.
        /// </summary>
        End,
    }
}