using System;
using Autofac;
using Reusable.Wiretap.Abstractions;

namespace Reusable.Wiretap
{
    public class LoggerModule : Module
    {
        private readonly ILoggerFactory _loggerFactory;

        public LoggerModule(ILoggerFactory loggerFactory)
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

    public static class RegistrationExtensions
    {
        public static void RegisterOmniLog(this ContainerBuilder builder, ILoggerFactory loggerFactory)
        {
            builder
                .RegisterInstance(loggerFactory)
                .ExternallyOwned()
                .As<ILoggerFactory>();

            builder
                .RegisterGeneric(typeof(Logger<>))
                .As(typeof(ILogger<>));
        }
    }
}