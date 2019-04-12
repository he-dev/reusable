using Autofac;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.DependencyInjection
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterLogger(this ContainerBuilder builder, ILoggerFactory loggerFactory)
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