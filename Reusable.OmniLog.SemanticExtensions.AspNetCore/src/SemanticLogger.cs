using System;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Reusable.Beaver;
using Reusable.Data;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;

namespace Reusable.OmniLog.SemanticExtensions.AspNetCore
{
    [UsedImplicitly]
    [PublicAPI]
    public class SemanticLogger
    {
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;
        private readonly SemanticLoggerConfig _config;

        public SemanticLogger
        (
            ILoggerFactory loggerFactory,
            RequestDelegate next,
            SemanticLoggerConfig config
        )
        {
            _next = next;
            _config = config;
            _logger = loggerFactory.CreateLogger<SemanticLogger>();
        }

        public async Task Invoke(HttpContext context, IFeatureToggle featureToggle)
        {
            using (_logger.BeginScope(_config.GetCorrelationId(context)).WithCorrelationHandle("HandleRequest").UseStopwatch())
            {
                _logger.Log(Abstraction.Layer.Network().Subject(new { Path = context.Request.Path.ToString() }));
                _logger.Log(Abstraction.Layer.Service().Meta(new { UserAgent = context.Request.Headers.TryGetValue("User-Agent", out var userAgent) ? userAgent.First() : "Unknown" }));
                _logger.Log(Abstraction.Layer.Network().Meta(new { Request = _config.CreateRequestSnapshot(context) }));

                try
                {
                    var body = default(string);
                    var bodyBackup = context.Response.Body;

                    using (var memory = new MemoryStream())
                    {
                        context.Response.Body = memory;

                        await _next(context);

                        using (var reader = new StreamReader(memory.Rewind()))
                        {
                            body = featureToggle.IsEnabled(Features.LogResponseBody) ? await reader.ReadToEndAsync() : default;

                            // Restore Response.Body
                            if (!context.Response.StatusCode.In(304))
                            {
                                await memory.Rewind().CopyToAsync(bodyBackup);
                            }

                            context.Response.Body = bodyBackup;
                        }
                    }

                    _logger.Log(Abstraction.Layer.Network().Meta(new
                    {
                        Response = _config.CreateResponseSnapshot(context)
                    }), log =>
                    {
                        log.Level(MapStatusCode(context.Response.StatusCode));
                        if (body is {})
                        {
                            log.Message(body);
                        }
                    });
                }
                catch (Exception inner)
                {
                    _logger.Log(Abstraction.Layer.Network().Routine("Request").Faulted(), inner);
                    throw;
                }
            }
        }

        private static Option<LogLevel> MapStatusCode(int statusCode)
        {
            return statusCode switch
            {
                var x when x >= 500 => LogLevel.Fatal,
                var x when x >= 400 => LogLevel.Error,
                var x when x >= 300 => LogLevel.Warning,
                _ => LogLevel.Information,
            };
        }
    }
}