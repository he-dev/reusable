using System;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Reusable.OmniLog.SemanticExtensions.AspNetCore.Extensions;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore
{
    [PublicAPI]
    public class SemanticLoggerConfig
    {
        public Func<HttpContext, object> GetCorrelationId { get; set; } = context => context.GetCorrelationId();

        public Func<HttpContext, object> CreateRequestSnapshot { get; set; } = context => new
        {
            Path = context.Request.Path.Value,
            Host = context.Request.Host.Value,
            context.Request.ContentLength,
            context.Request.ContentType,
            //context.Request.Cookies,
            context.Request.Headers,
            context.Request.IsHttps,
            context.Request.Method,
            context.Request.Protocol,
            context.Request.QueryString,
        };

        public Func<HttpContext, object> CreateResponseSnapshot { get; set; } = context => new
        {
            context.Response.ContentLength,
            context.Response.ContentType,
            context.Response.Headers,
            context.Response.StatusCode,
        };

        //public Func<HttpContext, bool> CanLogResponseBody { get; set; } = _ => true;
    }
}