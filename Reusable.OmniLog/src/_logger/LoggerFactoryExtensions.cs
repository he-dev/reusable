using System;
using System.IO;
using JetBrains.Annotations;
using Reusable.Extensions;
using Reusable.Flexo.Extensions;

namespace Reusable.OmniLog
{
    public static class LoggerFactoryExtensions
    {
        [NotNull]
        public static ILogger CreateLogger<T>([NotNull] this ILoggerFactory loggerFactory, bool includeNamespace = false)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            return loggerFactory.CreateLogger(typeof(T).ToPrettyString(includeNamespace));
        }

        #region Builder extensions

        public static LoggerFactory Attach(this LoggerFactory loggerFactory, string name, Func<ILog, object> action)
        {
            loggerFactory.Configuration.Attachements.Add(new OmniLog.Attachements.Lambda(name, action));
            return loggerFactory;
        }

        public static LoggerFactory Attach<T>(this LoggerFactory loggerFactory) where T : ILogAttachement, new()
        {
            loggerFactory.Configuration.Attachements.Add(new T());
            return loggerFactory;
        }

        public static LoggerFactory Attach<T>(this LoggerFactory loggerFactory, T attachment) where T : ILogAttachement
        {
            loggerFactory.Configuration.Attachements.Add(attachment);
            return loggerFactory;
        }

        public static LoggerFactory AttachObject(this LoggerFactory loggerFactory, string name, object value)
        {
            loggerFactory.Configuration.Attachements.Add(new OmniLog.Attachements.Lambda(name, _ => value));
            return loggerFactory;
        }

        public static LoggerFactory AddObserver(this LoggerFactory loggerFactory, [NotNull] ILogRx rx)
        {
            loggerFactory.Observers.Add(rx ?? throw new ArgumentNullException(nameof(rx)));
            return loggerFactory;
        }

        public static LoggerFactory AddObserver<TRx>(this LoggerFactory loggerFactory) where TRx : ILogRx, new()
        {
            loggerFactory.Observers.Add(new TRx());
            return loggerFactory;
        }

        public static LoggerFactory UseConfiguration(this LoggerFactory loggerFactory, LoggerFactoryConfiguration configuration)
        {
            loggerFactory.Configuration.LogPredicateExpression = configuration.LogPredicateExpression;
            return loggerFactory;
        }

        public static LoggerFactory UseConfiguration(this LoggerFactory loggerFactory, string fileName)
        {
            return loggerFactory.UseConfiguration(LoggerFactoryConfiguration.Load(fileName));
        }

        public static LoggerFactory UseConfiguration(this LoggerFactory loggerFactory, Stream jsonStream)
        {
            return loggerFactory.UseConfiguration(LoggerFactoryConfiguration.Load(jsonStream));
        }

        #endregion
    }
}