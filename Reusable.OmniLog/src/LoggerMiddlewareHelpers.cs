using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Middleware;

namespace Reusable.OmniLog
{
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
        public static void UseLambda(this ILogger logger, AlterLog alter)
        {
            LoggerLambda.Push(new LoggerLambda.Item { Alter = alter });
        }
    }

    public static class LoggerTransactionHelper
    {
        public static Middleware.LoggerTransaction.Scope UseTransaction(this ILogger logger)
        {
            return
                logger
                    .Middleware
                    .Enumerate(m => m.Next)
                    .OfType<LoggerTransaction>()
                    .Single()
                    .Push(default);
            //return logger.Use(new Middleware.LoggerTransaction());
        }
    }

//    public static class LoggerFilterHelper
//    {
//        public static LoggerFilter UseFilter(this ILogger logger, Func<ILog, bool> canLog)
//        {
//            return logger.Use(new LoggerFilter(canLog));
//        }
//    }




}