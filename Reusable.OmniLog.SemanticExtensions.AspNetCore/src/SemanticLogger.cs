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
                    var responseBody = default(string);
                    var responseBodyOriginal = context.Response.Body;

                    using (var memory = new MemoryStream())
                    {
                        context.Response.Body = memory;

                        await _next(context);

                        using (var reader = new StreamReader(memory.Rewind()))
                        {
                            responseBody = await featureController.Use(Features.LogResponseBody, async () => await reader.ReadToEndAsync());

                            if (_config.CanUpdateOriginalResponseBody(context))
                            {
                                // Update the original response-body.
                                await memory.Rewind().CopyToAsync(responseBodyOriginal);
                            }

                            // Restore the original response-body.
                            context.Response.Body = responseBodyOriginal;
                        }
                    }

                    _config.LogResponse(_logger, context, responseBody);
                }
                catch (Exception inner)
                {
                    _config.LogError(_logger, context, inner);
                    throw;
                }
            }
        }

        public static class Features
        {
            public const string LogResponseBody = nameof(LogResponseBody);
        }
    }
}