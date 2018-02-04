using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.OmniLog.Collections;
using Reusable.OmniLog.SemanticExtensions.Attachements;

namespace Reusable.OmniLog.SemanticExtensions
{
    [PublicAPI]
    public static class LoggerExtensions
    {
        // We use context as the name and not abstractionContext because it otherwise interfers with intellisense.
        // The name abstractionContext appears first on the list and you need to scroll to get the Abstraction.
        public static void Log(
            this ILogger logger,
            IAbstractionContext context,
            Action<Log> logAction = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            foreach (var dump in Reflection.GetProperties(context.Dump))
            {
                logger.Log(context.LogLevel, log =>
                {
                    log.With("Category", context.CategoryName);
                    log.With("Identifier", dump.Key);

                    //if (!log.ContainsKey(nameof(LogBag)))
                    //{
                    //    log.Add(nameof(LogBag), new LogBag());
                    //}

                    log.Add(nameof(Snapshot) + nameof(Object), dump.Value);

                    log.With("Layer", context.LayerName);
                    log.Add(LogProperty.CallerMemberName, callerMemberName);
                    log.Add(LogProperty.CallerLineNumber, callerLineNumber);
                    log.Add(LogProperty.CallerFilePath, Path.GetFileName(callerFilePath));

                    logAction?.Invoke(log);
                });
            }
        }

        ///// <summary>
        ///// Begins a new transaction-scope with attached Elapsed.
        ///// </summary>
        //public static LogScope BeginTransaction(this ILogger logger, object state, Action<Log> logAction = null)
        //{
        //    logger.Log(Abstraction.Layer.Logging().Action().Started("Transaction"));

        //    return logger.BeginScope(null, new { Transaction = state }, logAction ?? (log => log.Elapsed()));
        //}

        //public static void Commit(this ILogger logger)
        //{
        //    if (LogScope.Current is null)
        //    {
        //        throw new InvalidOperationException("Commit can be called only within a log-scope.");
        //    }

        //    logger.Log(Abstraction.Layer.Logging().Action().Finished("Transaction"));
        //}
    }

}
