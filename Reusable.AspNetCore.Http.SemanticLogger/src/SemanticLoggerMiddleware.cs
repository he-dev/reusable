using System;
using System.IO;
using System.Linq.Custom;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Reusable.OmniLog;
using Reusable.OmniLog.Collections;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.AspNetCore.Http
{
    public class SemanticLoggerMiddleware
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly RequestDelegate _next;
        private readonly SemanticLoggerMiddlewareConfiguration _configuration;

        public SemanticLoggerMiddleware(
            ILoggerFactory loggerFactory,
            RequestDelegate next,
            Action<SemanticLoggerMiddlewareConfiguration> configure)
        {
            _next = next;
            _loggerFactory = loggerFactory;
            _configuration = new SemanticLoggerMiddlewareConfiguration();
            configure(_configuration);
        }

        public async Task Invoke(HttpContext context)
        {
            /*
             The default Logger property is used to redirect logs into product table.
             The ProductLogger property overrides the Logger property by providing a value 
             that is logged into the Logger column instead of the table name.
             */

            var product = _configuration.GetProduct(context);
            var correlationId = _configuration.GetCorrelationId(context);
            var logger = _loggerFactory.CreateLogger(_configuration.MapProduct(product));

            var scope = logger.BeginScope().WithCorrelationId(correlationId).AttachElapsed();

            logger.Log(Abstraction.Layer.Network().Argument(new
            {
                Request = new
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
                }
            }), log =>
            {
                log.WithDisplayLogger(product);
            });

            try
            {
                var body = default(string);
                var bodyBackup = context.Response.Body;

                using (var memory = new MemoryStream())
                {
                    context.Response.Body = memory;

                    await _next(context);

                    memory.Seek(0, SeekOrigin.Begin);
                    using (var reader = new StreamReader(memory))
                    {
                        if (context.ResponseBodyLoggingEnabled())
                        {
                            body = await reader.ReadToEndAsync();
                        }

                        // Restore Response.Body
                        memory.Seek(0, SeekOrigin.Begin);
                        if (!context.Response.StatusCode.In(304))
                        {
                            await memory.CopyToAsync(bodyBackup);
                        }

                        context.Response.Body = bodyBackup;
                    }
                }

                logger.Log(Abstraction.Layer.Network().Argument(new
                {
                    Response = new
                    {
                        context.Response.ContentLength,
                        context.Response.ContentType,
                        context.Response.Headers,
                        context.Response.StatusCode,
                    }
                }), log =>
                {
                    log.Level(MapStatusCode(context.Response.StatusCode));
                    log.WithDisplayLogger(product);
                    if (context.ResponseBodyLoggingEnabled())
                    {
                        log.Message(body);
                    }
                });
            }
            catch (Exception ex)
            {
                logger.Log(Abstraction.Layer.Network().Routine("next").Faulted(), log =>
                {
                    log.Exception(ex);
                    log.WithDisplayLogger(product);
                });
                throw;
            }
            finally
            {
                scope.Dispose();
            }
        }

        private static LogLevel MapStatusCode(int statusCode)
        {
            if (statusCode >= 500)
            {
                return LogLevel.Fatal;
            }

            if (statusCode >= 400)
            {
                return LogLevel.Error;
            }

            if (statusCode >= 300)
            {
                return LogLevel.Warning;
            }

            return LogLevel.Information;
        }
    }

    internal static class LogExtensions
    {
        public static ILog WithDisplayLogger(this ILog log, string product)
        {
            return log.With("DisplayLogger", product);
        }
    }

    public static class HttpContextExtensions
    {
        public static void EnableResponseBodyLogging(this HttpContext context)
        {
            context.Items[nameof(ResponseBodyLoggingEnabled)] = true;
        }

        public static bool ResponseBodyLoggingEnabled(this HttpContext context)
        {
            return context.Items[nameof(ResponseBodyLoggingEnabled)] is bool enabled && enabled;
        }
    }
}
