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

        public async Task Invoke(HttpContext context, IFeatureController featureController)
        {
            using (_logger.BeginScope(_config.GetCorrelationId(context)).WithCorrelationHandle(_config.GetCorrelationHandle(context)).UseStopwatch())
            {
                var requestBody =
                    _config.CanLogRequestBody(context)
                        ? await _config.SerializeRequestBody(context)
                        : default;


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
                            body = await featureController.Use(Features.LogResponseBody, async () => await reader.ReadToEndAsync());

                            // Restore Response.Body
                            if (!context.Response.StatusCode.In(304))
                            {
                                await memory.Rewind().CopyToAsync(bodyBackup);
                            }

                            context.Response.Body = bodyBackup;
                        }
                    }

                    _config.LogResponse(_logger, context, featureController.Use(Features.LogResponseBody, body));
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