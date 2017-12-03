using System;
using JetBrains.Annotations;
using Reusable.Extensions;

namespace Reusable.OmniLog
{
    public static class LoggerFactoryExtensions
    {
        [NotNull]
        public static ILogger CreateLogger<T>([NotNull] this ILoggerFactory loggerFactory, bool fullName = false)
        {
            if (loggerFactory == null) throw new ArgumentNullException(nameof(loggerFactory));

            return loggerFactory.CreateLogger(typeof(T).ToPrettyString().IIf(_ => fullName, x => x, x => x.ToShortName()));
        }
    }
}