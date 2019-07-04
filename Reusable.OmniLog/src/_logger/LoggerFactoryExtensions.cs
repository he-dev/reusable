using System;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Attachments;
using Reusable.Utilities.JsonNet.Converters;

namespace Reusable.OmniLog
{
    public static class LoggerFactoryExtensions
    {
        [NotNull]
        public static ILogger<T> CreateLogger<T>([NotNull] this ILoggerFactory loggerFactory, bool includeNamespace = false)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            return new Logger<T>(loggerFactory);
        }

        #region Builder extensions

        public static LoggerFactory Attach(this LoggerFactory loggerFactory, string name, Func<ILog, object> action)
        {
            loggerFactory.Attachments.Add(new Lambda(name, action));
            return loggerFactory;
        }

        public static LoggerFactory Attach<T>(this LoggerFactory loggerFactory) where T : ILogAttachment, new()
        {
            loggerFactory.Attachments.Add(new T());
            return loggerFactory;
        }

        public static LoggerFactory Attach<T>(this LoggerFactory loggerFactory, T attachment) where T : ILogAttachment
        {
            loggerFactory.Attachments.Add(attachment);
            return loggerFactory;
        }

        public static LoggerFactory AttachObject(this LoggerFactory loggerFactory, string name, object value)
        {
            loggerFactory.Attachments.Add(new Lambda(name, _ => value));
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

//        public static LoggerFactory UseConfiguration(this LoggerFactory loggerFactory, LoggerFactoryConfiguration configuration)
//        {
//            loggerFactory.Configuration.LogPredicateExpression = configuration.LogPredicateExpression;
//            return loggerFactory;
//        }

//        public static LoggerFactory UseConfiguration(this LoggerFactory loggerFactory, string fileName)
//        {
//            return loggerFactory.UseConfiguration(LoggerFactoryConfiguration.Load(fileName));
//        }

//        public static LoggerFactory UseConfiguration(this LoggerFactory loggerFactory, Stream jsonStream)
//        {
//            return loggerFactory.UseConfiguration(LoggerFactoryConfiguration.Load(jsonStream));
//        }

        public static LoggerFactory UseConverter<T>(this LoggerFactory loggerFactory, WriteJsonCallback<T> convert)
        {
            var converter = new SimpleJsonConverter<T>(convert);
            return loggerFactory;
        }

        #endregion
    }
}