using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.Exceptionize;
using Reusable.Extensions;
using Reusable.OmniLog.Collections;
using Reusable.OmniLog.SemanticExtensions.Attachements;
// ReSharper disable ExplicitCallerInfoArgument - yes, we want to explicity set it via overloads.

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
            foreach (var dump in context.Dump.EnumerateProperties())
            {
                logger.Log(context.LogLevel, log =>
                {
                    // It's ok to hardcode these property names here because this is the only place they are used.

                    log.With("Category", context.CategoryName);
                    log.With("Identifier", dump.Key);

                    log.Add(nameof(Snapshot) + nameof(Object), dump.Value);

                    log.With("Layer", context.LayerName);
                    log.Add(LogProperties.CallerMemberName, callerMemberName);
                    log.Add(LogProperties.CallerLineNumber, callerLineNumber);
                    log.Add(LogProperties.CallerFilePath, Path.GetFileName(callerFilePath));

                    logAction?.Invoke(log);
                });
            }
        }

        public static void Log(
            this ILogger logger,
            IAbstractionContext context,
            string message,
            Exception exception,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Log(context, log =>
            {
                log.Message(message);
                log.Exception(exception);
            }, callerMemberName, callerLineNumber, callerFilePath);
        }

        public static void Log(
            this ILogger logger,
            IAbstractionContext context,
            string message,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Log(context, log =>
            {
                log.Message(message);
            }, callerMemberName, callerLineNumber, callerFilePath);
        }

        public static void Log(
            this ILogger logger,
            IAbstractionContext context,
            Exception exception,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Log(context, log =>
            {
                log.Exception(exception);
            }, callerMemberName, callerLineNumber, callerFilePath);
        }
    }
}
