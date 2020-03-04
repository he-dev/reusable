using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore.Helpers
{
    public static class LogHelper
    {
        /// <summary>
        /// Maps http-status-code to OmiLog log-level.
        /// </summary>
        public static Func<int, LogLevel> MapStatusCode { get; set; } = statusCode =>
        {
            return statusCode switch
            {
                var x when x >= 500 => LogLevel.Fatal,
                var x when x >= 400 => LogLevel.Error,
                var x when x >= 300 => LogLevel.Warning,
                _ => LogLevel.Information,
            };
        };

        public static void LogRequest(ILogger logger, HttpContext context, string? body)
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
        }

        public static void LogResponse(ILogger logger, HttpContext context, string? body)
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
                log.Level(LogHelper.MapStatusCode(context.Response.StatusCode));
                if (body is {})
                {
                    log.Message(body);
                }
            });
        }

        public static void LogError(ILogger logger, HttpContext context, Exception exception)
        {
            logger.Log(Abstraction.Layer.Network().Routine("HttpRequest").Faulted(), exception);
        }


        public static async Task<string?> SerializeRequestBody(this HttpContext context)
        {
            if (context.Request.ContentLength > 0)
            {
                try
                {
                    using var requestCopy = new MemoryStream();
                    using var requestReader = new StreamReader(requestCopy);
                    context.Request.EnableRewind();
                    await context.Request.Body.CopyToAsync(requestCopy);
                    requestCopy.Rewind();
                    return await requestReader.ReadToEndAsync();
                }
                finally
                {
                    context.Request.Body.Rewind();
                }
            }
            else
            {
                return default;
            }
        }
    }
}