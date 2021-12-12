using System.Collections.Generic;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Reusable.OmniLog.Utilities.AspNetCore;
using Reusable.OmniLog.Utilities.AspNetCore.Mvc.Filters;
using Reusable.Wiretap.Abstractions;
using Reusable.Wiretap.Extensions;

namespace Reusable.Wiretap.Utilities.AspNetCore;

public static class SemanticLoggerExtensions
{
    /// <summary>
    /// Adds OmniLog service to the container.
    /// </summary>
    public static IServiceCollection AddWiretap(this IServiceCollection services, ILoggerFactory loggerFactory)
    {
        return 
            services
                .AddSingleton(loggerFactory)
                .AddSingleton(typeof(ILogger<>), typeof(Logger<>))
                .AddScoped<LogResponseBody>();
    }
        
    public static IServiceCollection AddWiretap(this IServiceCollection services, IEnumerable<ILoggerNode> nodes)
    {
        //return services.AddWiretap(nodes.ToLoggerFactory());
        return default;
    }
        
    /// <summary>
    /// Registers OmniLog logger.
    /// </summary>
    public static IApplicationBuilder UseWiretap(this IApplicationBuilder builder, SemanticLoggerConfig? config = default)
    {
        return builder.UseMiddleware<SemanticLogger>(config ?? new SemanticLoggerConfig());
    }
}