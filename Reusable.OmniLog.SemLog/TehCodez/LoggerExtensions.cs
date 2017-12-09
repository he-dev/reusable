using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using JetBrains.Annotations;
using Reusable.OmniLog.Collections;
using Reusable.OmniLog.SemanticExtensions.Attachements;

namespace Reusable.OmniLog.SemanticExtensions
{
    public delegate (string Name, object Object) CreateCategoryFunc(Log log);    

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
            CreateCategoryFunc createCategoryFunc,
            Layer layer,
            Action<Log> logAction = null,
            [CallerMemberName] string callerMemberName = null,
            [CallerLineNumber] int callerLineNumber = 0,
            [CallerFilePath] string callerFilePath = null)
        {
            logger.Log(LogLevelMap[layer], log =>
            {
                var category = createCategoryFunc(log);

                log.With(nameof(Category), category.Name);               

                if (!log.ContainsKey("Bag"))
                {
                    log.Add("Bag", new LogBag());
                }
                log.Bag().Add(nameof(Snapshot), category.Object);

                log.With(nameof(Layer), layer);
                log.Add(LogProperty.CallerMemberName, callerMemberName);
                log.Add(LogProperty.CallerLineNumber, callerLineNumber);
                log.Add(LogProperty.CallerFilePath, Path.GetFileName(callerFilePath));

                logAction?.Invoke(log);
            });
        }
    }
}
