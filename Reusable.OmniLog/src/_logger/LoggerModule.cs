using System;
using Autofac;
using JetBrains.Annotations;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog
{
    public class LoggerModule : Module
    {
        private readonly ILoggerFactory _loggerFactory;

        public LoggerModule([NotNull] ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterInstance(_loggerFactory)
                .ExternallyOwned()
                .As<ILoggerFactory>();

            builder
                .RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>));
        }
    }
}