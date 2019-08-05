using System;
using System.Linq;
using System.Reactive;
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

        

        public static LoggerFactory UseConverter<T>(this LoggerFactory loggerFactory, WriteJsonCallback<T> convert)
        {
            var converter = new SimpleJsonConverter<T>(convert);
            return loggerFactory;
        }

    }
}