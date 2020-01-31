using System;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.SemanticExtensions.AspNetCore.Extensions;
using Reusable.OmniLog.SemanticExtensions.AspNetCore.Helpers;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore
{
    [PublicAPI]
    public class SemanticLoggerConfig
    {
        public Func<HttpContext, object> GetCorrelationId { get; set; } = context => context.GetCorrelationId();

        public Func<HttpContext, object> GetCorrelationHandle { get; set; } = _ => "HttpRequest";

        public Func<HttpContext, bool> CanLogRequestBody { get; set; } = _ => true;
        
        public Func<HttpContext, bool> CanLogResponseBody { get; set; } = _ => true;

        public Action<ILogger, HttpContext, string> LogRequest { get; set; } = (logger, context, body) =>
        {
            logger.Log(Abstraction.Layer.Network().Subject(new
            {
                HttpRequest = new
                {
                    Path = context.Request.Path.Value,
                    Host = context.Request.Host.Value,
                    context.Request.ContentLength,
                    context.Request.ContentType,
                    context.Request.Cookies,
                    context.Request.Headers,
                    context.Request.IsHttps,
                    context.Request.Method,
                    context.Request.Protocol,
                    context.Request.QueryString,
                }
            }), log =>
            {
                if (body is {})
                {
                    log.Message(body);
                }
            });
        };

        public Action<ILogger, HttpContext, string> LogResponse { get; set; } = (logger, context, body) =>
        {
            logger.Log(Abstraction.Layer.Network().Meta(new
            {
                HttpResponse = new
                {
                    context.Response.ContentLength,
                    context.Response.ContentType,
                    context.Response.Headers,
                    context.Response.StatusCode,
                }
            }), log =>
            {
                log.Level(HttpHelper.MapStatusCode(context.Response.StatusCode));
                if (body is {})
                {
                    log.Message(body);
                }
            });
        };

        public Action<ILogger, HttpContext, Exception> LogError { get; set; } = (logger, context, exception) => { logger.Log(Abstraction.Layer.Network().Routine("HttpRequest").Faulted(), exception); };
    }
}