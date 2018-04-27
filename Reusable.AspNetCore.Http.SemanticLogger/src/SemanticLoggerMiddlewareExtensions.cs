using System;
using Microsoft.AspNetCore.Builder;

namespace Reusable.AspNetCore.Http
{
    public static class SemanticLoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseSemanticLogger(this IApplicationBuilder builder, Action<SemanticLoggerMiddlewareConfiguration> configure)
        {
            return builder.UseMiddleware<SemanticLoggerMiddleware>(configure);
        }
    }
}