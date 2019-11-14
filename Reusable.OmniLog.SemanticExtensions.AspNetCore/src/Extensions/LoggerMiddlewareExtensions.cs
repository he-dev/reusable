using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore.Extensions
{
    public static class LoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseOmniLog(this IApplicationBuilder builder, SemanticLoggerConfig? config = default)
        {
            return builder.UseMiddleware<SemanticLogger>(config ?? new SemanticLoggerConfig());
        }
    }
}