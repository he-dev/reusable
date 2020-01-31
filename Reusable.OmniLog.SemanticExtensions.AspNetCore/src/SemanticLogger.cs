using System;
using System.IO;
using System.Linq;
using System.Linq.Custom;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Reusable.Beaver;
using Reusable.Extensions;
using Reusable.OmniLog.Abstractions;
using Reusable.OmniLog.Nodes;
using Reusable.OmniLog.SemanticExtensions.AspNetCore.Mvc.Filters;

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

        public async Task Invoke(HttpContext context, IFeatureAgent featureAgent)
        {
            using (_logger.BeginScope(_config.GetCorrelationId(context)).WithCorrelationHandle(_config.GetCorrelationHandle(context)).UseStopwatch())
            {
                var requestBody = default(string);
                
                if (_config.CanLogRequestBody(context) && context.Request.ContentLength > 0)
                {
                    using var requestCopy = new MemoryStream();
                    using var requestReader = new StreamReader(requestCopy);
                    context.Request.EnableRewind();
                    await context.Request.Body.CopyToAsync(requestCopy);
                    requestCopy.Rewind();
                    requestBody = await requestReader.ReadToEndAsync();
                    context.Request.Body.Rewind();
                }

                _config.LogRequest(_logger, context, requestBody);

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
                            body = await featureAgent.Use(Features.LogResponseBody, async () => await reader.ReadToEndAsync());

                            // Restore Response.Body
                            if (!context.Response.StatusCode.In(304))
                            {
                                await memory.Rewind().CopyToAsync(bodyBackup);
                            }

                            context.Response.Body = bodyBackup;
                        }
                    }

                    _config.LogResponse(_logger, context, featureAgent.Use(Features.LogResponseBody, body));
                }
                catch (Exception inner)
                {
                    _config.LogError(_logger, context, inner);
                    throw;
                }
            }
        }
    }
}