using System.Linq;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Abstractions.Data;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog
{
    public static class LoggerStopwatchHelper
    {
        public static StopwatchNode.Scope UseStopwatch(this ILogger logger)
        {
            return
                logger
                    .Node
                    .Enumerate(m => m.Next)
                    .OfType<StopwatchNode>()
                    .Single()
                    .Push(default);
        }
    }

    public static class LoggerLambdaHelper
    {
        public static void UseLambda(this ILogger logger, AlterLogEntryCallback alter)
        {
            LambdaNode.Push(new LambdaNode.Item { AlterLogEntry = alter });
        }
    }

    public static class LoggerTransactionHelper
    {
        public static TransactionNode.Scope UseTransaction(this ILogger logger)
        {
            return
                logger
                    .Node
                    .Enumerate(m => m.Next)
                    .OfType<TransactionNode>()
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