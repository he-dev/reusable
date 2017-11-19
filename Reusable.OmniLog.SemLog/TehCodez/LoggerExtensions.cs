using System;
using System.Collections.Generic;
using System.IO;
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
            [Layer.Business] = LogLevel.Information,
            [Layer.Application] = LogLevel.Debug,
            [Layer.Presentation] = LogLevel.Trace,
            [Layer.IO] = LogLevel.Trace,
            [Layer.Database] = LogLevel.Trace,
            [Layer.Network] = LogLevel.Trace,
            [Layer.External] = LogLevel.Trace,
        };

        public static void State(
            this ILogger logger, 
            Layer layer, 
            Action<StateBuilder> state, 
            string message = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Log(LogLevelMap[layer], log =>
            {
                log.With(nameof(Layer), layer);

                var stateBuilder = new StateBuilder();
                state(stateBuilder);

                var (expected, actual) = stateBuilder;

                if ((!(expected is null) || !(actual is null)) && log.Bag() is null)
                {
                    log.Bag(new LogBag());
                }

                if (!(expected is null))
                {
                    log.Bag().Add(nameof(Expected), expected);
                }

                if (!(actual is null))
                {
                    log.Bag().Add(nameof(Actual), actual);
                }

                if (!(message is null))
                {
                    log.Message(message);
                }

                log.Add(LogProperty.CallerMemberName, callerMemberName);
                log.Add(LogProperty.CallerLineNumber, callerLineNumber);
                log.Add(LogProperty.CallerFilePath, Path.GetFileName(callerFilePath));
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
                log.Add(LogProperty.CallerFilePath, Path.GetFileName(callerFilePath));
            });
        }
    }

    public class StateBuilder
    {
        private object _expected;
        private object _actual;

        public StateBuilder Expected(object expected)
        {
            _expected = expected;
            return this;
        }

        public StateBuilder Actual(object actual)
        {
            _actual = actual;
            return this;
        }

        public void Deconstruct(out object expected, out object actual)
        {
            expected = _expected;
            actual = _actual;
        }
    }
}
