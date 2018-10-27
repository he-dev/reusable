using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace Reusable.AspNetCore.Http
{
    public class SemanticLoggerMiddlewareConfiguration
    {
        public SemanticLoggerMiddlewareConfiguration()
        {
            //GetProduct = context => context.Request.Headers["X-Product"].SingleOrDefault() ?? "Unknown";
            GetCorrelationContext = context =>
            {
                var name = context.Request.Headers["X-Correlation-Context"].ElementAtOrDefault(0);
                var value = context.Request.Headers["X-Correlation-Context"].ElementAtOrDefault(1);

                return
                    string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(value)
                        ? null
                        : new Dictionary<string, string> {[name] = value};

                //return correlationContext is null ? null : new { Header = correlationContext };
            };
            GetCorrelationId = context => context.Request.Headers["X-Correlation-ID"].SingleOrDefault() ?? context.TraceIdentifier;
            MapProduct = product => product;
        }

        //public Func<HttpContext, string> GetProduct { get; set; }

        public Func<HttpContext, object> GetCorrelationContext { get; set; }

        public Func<HttpContext, string> GetCorrelationId { get; set; }

        public Func<string, string> MapProduct { get; set; }
    }
}