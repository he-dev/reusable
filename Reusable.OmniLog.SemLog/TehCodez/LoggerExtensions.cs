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
    //public delegate (string CategoryName, string ObjectName, object Object) CreateCategoryFunc(Log log);
    public delegate (string CategoryName, object Dump) CreateCategoryFunc(Log log);

    [PublicAPI]
    public static class LoggerExtensions
    {
        //private static readonly IDictionary<Layer, LogLevel> LogLevelMap = new Dictionary<Layer, LogLevel>
        //{
        //    [Layer.Business] = LogLevel.Information,
        //    [Layer.Application] = LogLevel.Debug,
        //    [Layer.Presentation] = LogLevel.Trace,
        //    [Layer.IO] = LogLevel.Trace,
        //    [Layer.Database] = LogLevel.Trace,
        //    [Layer.Network] = LogLevel.Trace,
        //    [Layer.External] = LogLevel.Trace,
        //};

        public static void Log(
            this ILogger logger,
            IAbstractionContext abstractionContext,
            Action<Log> logAction = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            foreach (var dump in Reflection.GetProperties(abstractionContext.Dump))
            {
                logger.Log(abstractionContext.LogLevel, log =>
                {
                    log.With(nameof(Category), abstractionContext.CategoryName);
                    log.With("Identifier", dump.PropertyName);

                    if (!log.ContainsKey(nameof(LogBag)))
                    {
                        log.Add(nameof(LogBag), new LogBag());
                    }

                    log.Bag().Add(nameof(Snapshot), dump);

                    log.With("Layer", abstractionContext.LayerName);
                    log.Add(LogProperty.CallerMemberName, callerMemberName);
                    log.Add(LogProperty.CallerLineNumber, callerLineNumber);
                    log.Add(LogProperty.CallerFilePath, Path.GetFileName(callerFilePath));

                    logAction?.Invoke(log);
                });
            }
        }

        /// <summary>
        /// Begins a new transaction-scope.
        /// </summary>
        public static LogScope BeginTransaction(this ILogger logger, object state, Action<Log> logAction = null)
        {
            return logger.BeginScope(null, new { Transaction = state }, logAction ?? (_ => { }));
        }
    }
}
