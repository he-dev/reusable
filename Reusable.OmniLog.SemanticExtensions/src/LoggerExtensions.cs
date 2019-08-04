using System;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;

// ReSharper disable ExplicitCallerInfoArgument - yes, we want to explicitly set it via overloads.

namespace Reusable.OmniLog.SemanticExtensions
{
    [PublicAPI]
    public static class LoggerExtensions
    {
        // We use context as the name and not abstractionContext because it otherwise interferes with intellisense.
        // The name abstractionContext appears first on the list and you need to scroll to get the Abstraction.
        public static void Log//<TContext>
        (
            this ILogger logger,
            IAbstractionContext context,
            Func<ILog, ILog> populate = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null
        ) //where TContext : IAbstractionContext
        {
            context.Log(logger, log =>
            {
                log.SetItem(Reusable.OmniLog.Log.PropertyNames.CallerMemberName, callerMemberName);
                log.SetItem(Reusable.OmniLog.Log.PropertyNames.CallerLineNumber, callerLineNumber);
                log.SetItem(Reusable.OmniLog.Log.PropertyNames.CallerFilePath, Path.GetFileName(callerFilePath));
                populate?.Invoke(log);
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
            logger.Log
            (
                context,
                log => log.SetItem(Reusable.OmniLog.Log.PropertyNames.Message, message).SetItem(Reusable.OmniLog.Log.PropertyNames.Exception, exception),
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
            logger.Log
            (
                context,
                log => log.SetItem(Reusable.OmniLog.Log.PropertyNames.Message, message),
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
            logger.Log
            (
                context,
                log => log.SetItem(Reusable.OmniLog.Log.PropertyNames.Exception, exception),
                callerMemberName,
                callerLineNumber,
                callerFilePath
            );
        }
    }
}