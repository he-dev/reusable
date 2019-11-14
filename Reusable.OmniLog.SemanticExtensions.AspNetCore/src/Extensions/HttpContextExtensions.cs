using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore.Extensions
{
    internal static class HttpContextExtensions
    {
        public static string GetCorrelationId(this HttpContext context, string header = "X-Correlation-ID")
        {
            return
                context.Request.Headers.TryGetValue(header, out var correlationIds)
                    ? correlationIds.First()
                    : context.TraceIdentifier;
        }
    }
}