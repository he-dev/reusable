using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOmniLog(this IServiceCollection services, Action<LoggerFactory> setupAction)
        {
            var loggerFactory = new LoggerFactory();
            setupAction(loggerFactory);
            services.TryAddSingleton<ILoggerFactory>(loggerFactory);
            return services;
        }
    }
}