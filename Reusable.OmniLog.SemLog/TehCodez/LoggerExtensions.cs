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
            Func<(string Name, object Object, string Message)> snapshot,
            LogLevel level,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            level = level ?? LogLevelMap[layer];
            logger.Log(level, log =>
            {
                var s = snapshot();

                log.With(nameof(Layer), layer);
                log.With("State", s.Name);

                if (!log.ContainsKey("Bag"))
                {
                    log.Add("Bag", new LogBag());
                }
                log.Bag().Add(nameof(Snapshot), s.Object);

                if (!(s.Message is null))
                {
                    log.Message(s.Message);
                }


                log.Add(LogProperty.CallerMemberName, callerMemberName);
                log.Add(LogProperty.CallerLineNumber, callerLineNumber);
                log.Add(LogProperty.CallerFilePath, Path.GetFileName(callerFilePath));
            });
        }

        public static void State(
            this ILogger logger,
            Layer layer,
            Func<(string Name, object Object)> snapshot,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.State(layer, () =>
            {
                var s = snapshot();
                return (s.Name, s.Object, null);
            }, null, callerMemberName, callerLineNumber, callerFilePath);
        }

        public static void State(
            this ILogger logger,
            Layer layer,
            Func<(string Name, object Object, string Message)> snapshot,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.State(layer, snapshot, null, callerMemberName, callerLineNumber, callerFilePath);
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
