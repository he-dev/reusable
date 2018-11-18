using System;
using Microsoft.AspNetCore.Builder;

namespace Reusable.OmniLog
{
    public static class LoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseSemanticLogger(this IApplicationBuilder builder, Action<LoggerMiddlewareConfiguration> configure)
        {
            return builder.UseMiddleware<LoggerMiddleware>(configure);
        }
    }
}