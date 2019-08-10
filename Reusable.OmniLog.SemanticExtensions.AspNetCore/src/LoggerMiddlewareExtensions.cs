using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore
{
    public static class LoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseOmniLog(this IApplicationBuilder builder, Func<HttpContext, object> getCorrelationId)
        {
            return builder.UseMiddleware<SemanticLogger>(getCorrelationId);
        }
    }
}