using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Reusable.OmniLog
{
    public static class LoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseSemanticLogger(this IApplicationBuilder builder, Func<HttpContext, object> getCorrelationId)
        {
            return builder.UseMiddleware<LoggerMiddleware>(getCorrelationId);
        }
    }
}