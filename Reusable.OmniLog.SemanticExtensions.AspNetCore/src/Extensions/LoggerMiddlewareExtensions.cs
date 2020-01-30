using Microsoft.AspNetCore.Builder;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore.Extensions
{
    public static class LoggerMiddlewareExtensions
    {
        /// <summary>
        /// Registers OmniLog logger.
        /// </summary>
        public static IApplicationBuilder UseOmniLog(this IApplicationBuilder builder, SemanticLoggerConfig? config = default)
        {
            return builder.UseMiddleware<SemanticLogger>(config ?? new SemanticLoggerConfig());
        }
    }
}