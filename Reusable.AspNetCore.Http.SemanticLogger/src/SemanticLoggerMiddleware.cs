using System;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Reusable.OmniLog;
using Reusable.OmniLog.Collections;
using Reusable.OmniLog.SemanticExtensions;

namespace Reusable.AspNetCore.Http
{
    public class SemanticLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILoggerFactory _loggerFactory;
        private readonly string _headerPrefix;
        private readonly Func<string, string> _createLoggerName;

        public SemanticLoggerMiddleware(
            RequestDelegate next,
            ILoggerFactory loggerFactory,
            string headerPrefix,
            Func<string, string> createLoggerName)
        {
            _next = next;
            _loggerFactory = loggerFactory;
            _headerPrefix = headerPrefix;
            _createLoggerName = createLoggerName;
        }

        public async Task Invoke(HttpContext context)
        {
            /*
             The default Logger property is used to redirect logs into product table.
             The ProductLogger property overrides the Logger property by providing a value 
             that is logged into the Logger column instead of the table name.
             */

            var product = context.Request.Headers[$"{_headerPrefix}Product"].SingleOrDefault();
            var logger = _loggerFactory.CreateLogger(_createLoggerName(product));
            var correlation = context.CorrelationObject();

            var displayLogger = product ?? nameof(SemanticLoggerMiddleware);

            using (logger.BeginScope("Pipeline", correlation).AttachElapsed())
            {
                logger.Log(Abstraction.Layer.Network().Argument(new
                {
                    context = new
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
                    }
                }), log =>
                {
                    log.WithDisplayLogger(displayLogger);
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
                        context = new
                        {
                            Response = new
                            {
                                context.Response.ContentLength,
                                context.Response.ContentType,
                                context.Response.Headers,
                                context.Response.StatusCode,
                            }
                        }
                    }), log =>
                    {
                        log.Level(MapStatusCode(context.Response.StatusCode));
                        log.WithDisplayLogger(displayLogger);
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
                        log.WithDisplayLogger(displayLogger);
                    });
                    throw;
                }
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

    public static class LogExtensions
    {
        public static ILog WithDisplayLogger(this ILog log, string product)
        {
            return log.With("DisplayLogger", product);
        }
    }

    public static class SemanticLoggerMiddlewareExtensions
    {
        public static IApplicationBuilder UseSemanticLogger(this IApplicationBuilder builder, string headerPrefix, Func<string, string> createLoggerName)
        {
            return builder.UseMiddleware<SemanticLoggerMiddleware>(headerPrefix, createLoggerName);
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

        public static object CorrelationObject(this HttpContext context)
        {
            return new
            {
                CorrelationId = context.Request.Headers["X-Correlation-Id"].SingleOrDefault() ?? context.TraceIdentifier
            };
        }
    }
}
