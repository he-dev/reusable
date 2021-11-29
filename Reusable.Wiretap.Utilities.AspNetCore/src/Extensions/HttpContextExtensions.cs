using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Reusable.OmniLog.Utilities.AspNetCore.Extensions
{
    public static class HttpContextExtensions
    {
        /// <summary>
        /// Gets the 'X-Correlation-ID' header by default, otherwise the 'TraceIdentifier'.
        /// </summary>
        public static string GetCorrelationHeaderOrDefault(this HttpContext context, string header = "X-Correlation-ID")
        {
            return context.Request.Headers[header] switch
            {
                {Count: 1} correlationId => correlationId.Single(),
                _ => context.TraceIdentifier
            };
        }
    }
}