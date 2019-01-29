using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;

// ReSharper disable ExplicitCallerInfoArgument - yes, we want to explicity set it via overloads.

namespace Reusable.OmniLog.SemanticExtensions
{
    [PublicAPI]
    public static class LoggerExtensions
    {
        // We use context as the name and not abstractionContext because it otherwise interfers with intellisense.
        // The name abstractionContext appears first on the list and you need to scroll to get the Abstraction.
        public static void Log<TContext>
        (
            this ILogger logger,
            TContext context,
            Action<Log> configureLog = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null
        ) where TContext : IAbstractionContext
        {
            context.Log(logger, log =>
            {
                log.Add(LogProperties.CallerMemberName, callerMemberName);
                log.Add(LogProperties.CallerLineNumber, callerLineNumber);
                log.Add(LogProperties.CallerFilePath, Path.GetFileName(callerFilePath));
                configureLog?.Invoke(log);
            });
        }

        public static void Log
        (
            this ILogger logger,
            IAbstractionContext context,
            string message,
            Exception exception,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Log(
                context,
                log =>
                {
                    log.Message(message);
                    log.Exception(exception);
                },
                callerMemberName,
                callerLineNumber,
                callerFilePath
            );
        }

        public static void Log
        (
            this ILogger logger,
            IAbstractionContext context,
            string message,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Log(
                context,
                log => { log.Message(message); },
                callerMemberName,
                callerLineNumber,
                callerFilePath
            );
        }

        public static void Log
        (
            this ILogger logger,
            IAbstractionContext context,
            Exception exception,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Log(
                context,
                log => { log.Exception(exception); },
                callerMemberName,
                callerLineNumber,
                callerFilePath
            );
        }
    }
}