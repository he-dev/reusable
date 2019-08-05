using System;
using System.Linq;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.v2
{
    using Reusable.OmniLog.v2;
    using Reusable.OmniLog.v2.Middleware;

    public static class LoggerStopwatchHelper
    {
        public static LoggerStopwatch.Scope UseStopwatch(this ILogger logger)
        {
            return
                logger
                    .Middleware
                    .Enumerate(m => m.Next)
                    .OfType<LoggerStopwatch>()
                    .Single()
                    .Push(default);
        }
    }

    public static class LoggerLambdaHelper
    {
        public static void UseLambda(this ILogger logger, Action<ILog> transform)
        {
            LoggerLambda.Push(new LoggerLambda.Item { Alter = transform });
        }
    }

    public static class LoggerTransactionHelper
    {
        public static Middleware.LoggerTransaction UseTransaction(this ILogger logger)
        {
            return logger.Use(new Middleware.LoggerTransaction());
        }
    }

//    public static class LoggerFilterHelper
//    {
//        public static LoggerFilter UseFilter(this ILogger logger, Func<ILog, bool> canLog)
//        {
//            return logger.Use(new LoggerFilter(canLog));
//        }
//    }

    public static class LoggerScopeHelper
    {
        public static LoggerCorrelation.Scope UseScope(this ILogger logger, object correlationId = default, object correlationHandle = default)
        {
            return
                logger
                    .Middleware
                    .Enumerate(m => m.Next)
                    .OfType<LoggerCorrelation>()
                    .Single()
                    .Push((correlationId, correlationHandle));
        }
    }

    public static class LoggerSerializerHelper
    {
        public static ILog AttachSerializable(this ILog log, string propertyName, object obj)
        {
            return log.SetItem(LoggerSerializer.CreateDataKey(propertyName), obj);
        }
    }
}