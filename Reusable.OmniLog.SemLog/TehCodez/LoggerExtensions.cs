using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.OmniLog.Collections;
using Reusable.OmniLog.SemLog.Attachements;

namespace Reusable.OmniLog.SemLog
{
    [PublicAPI]
    public static class Snapshot
    {
        public static Func<(string Name, object Object)> Properties<T>(object obj, string name = null)
        {
            return From(obj, nameof(Properties), typeof(T).ToPrettyString().ToShortName(), name);
        }

        public static Func<(string Name, object Object)> Properties(Type type, object obj, string name = null)
        {
            return From(obj, nameof(Properties), type.ToPrettyString().ToShortName(), name);
        }

        public static Func<(string Name, object Object)> Variables(object obj, string name = null)
        {
            return From(obj, nameof(Variables), name);
        }

        public static Func<(string Name, object Object)> Arguments(object obj, string name = null)
        {
            return From(obj, nameof(Arguments), name);
        }

        public static Func<(string Name, object Object)> From(object obj, params string[] path)
        {
            return () => (path.Where(Conditional.IsNotNullOrEmpty).Join("/"), obj);
        }
    }

    [PublicAPI]
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
            Func<(string Name, object Object)> snapshot,
            string message = null,
            LogLevel level = null,
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

        #region Scope state

        //public static void State(
        //    this ILogger logger,
        //    Func<(string Name, object Object)> snapshot,
        //    string message = null,
        //    LogLevel level = null,
        //    [CallerMemberName] string callerMemberName = null,
        //    [CallerLineNumber] int callerLineNumber = 0,
        //    [CallerFilePath] string callerFilePath = null)
        //{
        //    if (LogScope.Current.TryGetValue("Layer", out var layer))
        //    {
        //        logger.State((Layer)layer, snapshot, message, level, callerMemberName, callerLineNumber, callerFilePath);
        //    }
        //    else
        //    {
        //        throw new InvalidOperationException("You can use this method only within a scope with a Layer.");
        //    }
        //}

        #endregion

        #region Events by result

        public static void Success(
        this ILogger logger,
        Layer layer,
        string message = null,
        [CallerMemberName] string callerMemberName = null,
        [CallerLineNumber] int callerLineNumber = 0,
        [CallerFilePath] string callerFilePath = null)
        {
            logger.Event(layer, callerMemberName, Result.Success, message, null, callerMemberName, callerLineNumber, callerFilePath);
        }

        public static void Success(
            this ILogger logger,
            Layer layer,
            string name,
            string message = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Event(layer, name, Result.Success, message, null, callerMemberName, callerLineNumber, callerFilePath);
        }

        public static void Completed(
            this ILogger logger,
            Layer layer,
            string message = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Event(layer, callerMemberName, Result.Completed, message, null, callerMemberName, callerLineNumber, callerFilePath);
        }

        public static void Completed(
            this ILogger logger,
            Layer layer,
            string name,
            string message = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Event(layer, name, Result.Completed, message, null, callerMemberName, callerLineNumber, callerFilePath);
        }

        public static void Failure(
            this ILogger logger,
            Layer layer,
            Exception exception,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Event(layer, callerMemberName, Result.Failure, null, exception, callerMemberName, callerLineNumber, callerFilePath);
        }

        public static void Failure(
            this ILogger logger,
            Layer layer,
            string name,
            string message,
            Exception exception,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Event(layer, name, Result.Failure, message, exception, callerMemberName, callerLineNumber, callerFilePath);
        }

        public static void Failure(
            this ILogger logger,
            Layer layer,
            string name,
            Exception exception,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Event(layer, name, Result.Failure, null, exception, callerMemberName, callerLineNumber, callerFilePath);
        }

        public static void Failure(
            this ILogger logger,
            Layer layer,
            string name,
            string message = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Event(layer, name, Result.Failure, message, null, callerMemberName, callerLineNumber, callerFilePath);
        }

        #endregion
    }
}
