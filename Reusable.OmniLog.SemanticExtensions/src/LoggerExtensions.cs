using System;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Middleware;
using Reusable.OmniLog.SemanticExtensions.Middleware;

// ReSharper disable ExplicitCallerInfoArgument - yes, we want to explicitly set it via overloads.

namespace Reusable.OmniLog.SemanticExtensions
{
    using data = Reusable.OmniLog.Abstractions.Data;

    [PublicAPI]
    public static class LoggerExtensions
    {
        // We use context as the name and not abstractionContext because it otherwise interferes with intellisense.
        // The name abstractionContext appears first on the list and you need to scroll to get the Abstraction.
        public static void Log<T>
        (
            this ILogger logger,
            IAbstractionBuilder<T> context,
            AlterLog alter = null,
            // These properties are for free so let's just log them too.
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null
        )
        {
            logger.Log(log =>
            {
                log.SetItem(LoggerAbstraction.LogPropertyName, data.Log.ItemTags.Metadata, context);
                log.SetItem(data.Log.PropertyNames.CallerMemberName, default, callerMemberName);
                log.SetItem(data.Log.PropertyNames.CallerLineNumber, default, callerLineNumber);
                log.SetItem(data.Log.PropertyNames.CallerFilePath, default, Path.GetFileName(callerFilePath));
                alter?.Invoke(log);
            });
        }

        public static void Log<T>
        (
            this ILogger logger,
            IAbstractionBuilder<T> context,
            string message,
            Exception exception,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Log
            (
                context,
                log => log.Message(message).Exception(exception),
                callerMemberName,
                callerLineNumber,
                callerFilePath
            );
        }

        public static void Log<T>
        (
            this ILogger logger,
            IAbstractionBuilder<T> context,
            string message,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Log
            (
                context,
                log => log.Message(message),
                callerMemberName,
                callerLineNumber,
                callerFilePath
            );
        }

        public static void Log<T>
        (
            this ILogger logger,
            IAbstractionBuilder<T> context,
            Exception exception,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Log
            (
                context,
                log => log.Exception(exception),
                callerMemberName,
                callerLineNumber,
                callerFilePath
            );
        }
    }
}