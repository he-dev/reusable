using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Reusable.AspNetCore.Http
{
    public class SemanticLoggerMiddlewareConfiguration
    {
        public SemanticLoggerMiddlewareConfiguration()
        {
            GetProduct = context => context.Request.Headers["X-Product"].SingleOrDefault() ?? "Unknown";
            GetCorrelationId = context => context.Request.Headers["X-Correlation-ID"].SingleOrDefault() ?? context.TraceIdentifier;
            MapProduct = product => product;
        }

        public Func<HttpContext, string> GetProduct { get; set; }

        public Func<HttpContext, string> GetCorrelationId { get; set; }

        public Func<string, string> MapProduct { get; set; }
    }
}