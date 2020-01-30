using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.SemanticExtensions.AspNetCore.Mvc.Filters;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore.Extensions
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds OmniLog service to the container.
        /// </summary>
        public static IServiceCollection AddOmniLog(this IServiceCollection services, Action<LoggerFactoryBuilder> customize)
        {
            return 
                services
                    .AddSingleton(LoggerFactory.Builder().Pipe(customize).Build())
                    .AddSingleton(typeof(ILogger<>), typeof(Logger<>))
                    .AddScoped<LogResponseBody>();
        }
    }
}