using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.OmniLog.Collections;

namespace Reusable.OmniLog.SemanticExtensions
{
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

        public static void Log(
            this ILogger logger,
            Func<Log, (string Name, object Object)> categoryFunc,
            Layer layer,
            Action<Log> logAction = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Log(LogLevelMap[layer], log =>
            {
                var category = categoryFunc(log);

                log.With(nameof(Layer), layer);
                log.With(nameof(Category), category.Name);

                if (!(log.Exception() is null))
                {
                    log.LogLevel(LogLevel.Error);
                }

                if (!log.ContainsKey("Bag"))
                {
                    log.Add("Bag", new LogBag());
                }
                log.Bag().Add(nameof(Snapshot), category.Object);

                log.Add(LogProperty.CallerMemberName, callerMemberName);
                log.Add(LogProperty.CallerLineNumber, callerLineNumber);
                log.Add(LogProperty.CallerFilePath, Path.GetFileName(callerFilePath));

                logAction?.Invoke(log);
            });
        }        
    }
}
