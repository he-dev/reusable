using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Reusable.OmniLog.Collections;
using Reusable.OmniLog.SemLog.Attachements;

namespace Reusable.OmniLog.SemLog
{
    public static class LoggerExtensions
    {
        private static readonly IDictionary<Layer, LogLevel> LogLevelMap = new Dictionary<Layer, LogLevel>
        {
            [Layer.Presentation] = LogLevel.Trace,
            [Layer.Application] = LogLevel.Trace,
            [Layer.Business] = LogLevel.Debug,
            [Layer.IO] = LogLevel.Trace,
            [Layer.Database] = LogLevel.Trace,
            [Layer.Network] = LogLevel.Trace,
            [Layer.External] = LogLevel.Trace,
        };

        public static void State(this ILogger logger, Layer layer, Func<object> expected, Func<object> actual, string message = null)
        {
            logger.Log(LogLevelMap[layer], log =>
            {
                log.With(nameof(Layer), layer);

                if ((!(expected is null) || !(actual is null)) && log.Bag() is null)
                {
                    log.Bag(new LogBag());
                }
                if (!(expected is null))
                {
                    log.Bag().Add(nameof(Expected), expected());
                }
                if (!(actual is null))
                {
                    log.Bag().Add(nameof(Actual), actual());
                }
                if (!(message is null))
                {
                    log.Message(message);
                }
            });
        }

        public static void Event(
            this ILogger logger,
            Layer layer,
            string name,
            Result result,
            string message = null,
            Exception exception = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            var logLevel = LogLevelMap[layer];

            if (result == Result.Failure)
            {
                logLevel = LogLevel.Warning;
            }

            if (!(exception is null))
            {
                logLevel = LogLevel.Error;
            }

            logger.Log(logLevel, log =>
            {
                log.With(nameof(Layer), layer);
                log.With("Event", name);
                log.With(nameof(Result), result);
                if (!(message is null))
                {
                    log.Message(message);
                }
                if (!(exception is null))
                {
                    log.Exception(exception);
                }
                log.Add(LogProperty.CallerMemberName, callerMemberName);
                log.Add(LogProperty.CallerLineNumber, callerLineNumber);
                log.Add(LogProperty.CallerFilePath, callerFilePath);
            });
        }
    }
}
