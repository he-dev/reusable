using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Extensions;
using Reusable.OmniLog.Utilities.AspNetCore.Mvc.Filters;

namespace Reusable.OmniLog.Utilities.AspNetCore
{
    public static class SemanticLoggerExtensions
    {
        /// <summary>
        /// Adds OmniLog service to the container.
        /// </summary>
        public static IServiceCollection AddOmniLog(this IServiceCollection services, ILoggerFactory loggerFactory)
        {
            return 
                services
                    .AddSingleton(loggerFactory)
                    .AddSingleton(typeof(ILogger<>), typeof(Logger<>))
                    .AddScoped<LogResponseBody>();
        }
        
        public static IServiceCollection AddOmniLog(this IServiceCollection services, IEnumerable<ILoggerNode> nodes)
        {
            return services.AddOmniLog(nodes.ToLoggerFactory());
        }
        
        /// <summary>
        /// Registers OmniLog logger.
        /// </summary>
        public static IApplicationBuilder UseOmniLog(this IApplicationBuilder builder, SemanticLoggerConfig? config = default)
        {
            return builder.UseMiddleware<SemanticLogger>(config ?? new SemanticLoggerConfig());
        }
    }
}